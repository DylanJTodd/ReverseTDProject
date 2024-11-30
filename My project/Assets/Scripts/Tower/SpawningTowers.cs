using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SpawningTowers : MonoBehaviour
{
    public int startingMoney = 500;
    public int amountPerWave = 500;
    public float towerSpawnCooldown = 3f;

    public GameObject earthTowerPrefab;
    public GameObject lightningTowerPrefab;
    public GameObject fireTowerPrefab;
    public GameObject iceTowerPrefab;
    public GameObject spawnHolder;
    public GameObject monsterHolder;
    public GameObject pathsContainer;

    private int currentMoney;
    private List<GameObject> activeTowers = new List<GameObject>();
    private bool canSpawn = true;
    private Dictionary<Transform, float> spawnPointPathCoverage = new Dictionary<Transform, float>();
    private Dictionary<GameObject, Transform> towerSpawnPoints = new Dictionary<GameObject, Transform>();
    private int currentWave = 1;
    private bool canPerformAction = true;
    public float statMultiplier = 1f;


    void Start()
    {
        currentMoney = startingMoney;
        StartCoroutine(CalculatePathCoverageForSpawnPoints());
    }

    void Update()
    {
        if (!canSpawn) return;

        List<Monster> activeMonsters = GetActiveMonsters();
        if (activeMonsters.Count == 0) return;

        float currentDPS = CalculateTotalDPS();
        float requiredDPS = CalculateRequiredDPS(activeMonsters);

        if (currentDPS < requiredDPS * 1.2f)
        {
            HandleTowerPlacement();
        }
    }

    public void AddWaveMoney()
    {
        currentMoney += amountPerWave;
    }

    private struct MonsterAnalysis
    {
        public float avgHealth;
        public float avgSpeed;
        public int etherealCount;
        public int totalCount;
        public float maxHealth;
        public float totalHealth;
        public bool hasGroupedMonsters;
        public float avgDistance;
    }

    private class TowerEvaluation
    {
        public GameObject tower;
        public float baseScore;
        public float positionScore;
        public float threatScore;
        public float upgradeImpact;
        public float finalScore;
    }
    private struct ThreatAssessment
    {
        public float speedThreat;
        public float healthThreat;
        public float swarmThreat;
        public float etherealThreat;
        public float pathCoverageThreat;
    }
    private enum TowerPhase
    {
        Initial,
        Development,
        Reinforcement
    }

    private TowerPhase currentPhase = TowerPhase.Initial;
    private HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>();

    private IEnumerator CalculatePathCoverageForSpawnPoints()
    {
        foreach (Transform spawnPoint in spawnHolder.transform)
        {
            float coverage = CalculatePathCoverageForPoint(spawnPoint);
            spawnPointPathCoverage[spawnPoint] = coverage;
            yield return null;
        }
    }

    private ThreatAssessment AnalyzeThreats(List<Monster> monsters, float currentDPS)
    {
        if (monsters.Count == 0) return new ThreatAssessment();

        float avgSpeed = monsters.Average(m => m.GetSpeed());
        float maxSpeed = monsters.Max(m => m.GetSpeed());
        float baseSpeed = 2.0f;  // Slowest base monster speed

        float avgHealth = monsters.Average(m => m.GetHealth());
        float maxHealth = monsters.Max(m => m.GetHealth());
        float totalHealth = monsters.Sum(m => m.GetHealth());
        float timeToKill = totalHealth / (currentDPS > 0 ? currentDPS : 1);

        float monsterDensity = CalculateMonsterDensity(monsters);
        int etherealCount = monsters.Count(m => m.CantTarget());
        float pathProgression = CalculatePathProgression(monsters);

        return new ThreatAssessment
        {
            speedThreat = CalculateSpeedThreat(avgSpeed, maxSpeed, baseSpeed, pathProgression),
            healthThreat = CalculateHealthThreat(timeToKill, maxHealth, avgHealth),
            swarmThreat = CalculateSwarmThreat(monsters.Count, monsterDensity),
            etherealThreat = CalculateEtherealThreat(etherealCount, monsters.Count),
            pathCoverageThreat = CalculatePathCoverageThreat(monsters)
        };
    }

    private float CalculateSpeedThreat(float avgSpeed, float maxSpeed, float baseSpeed, float pathProgression)
    {
        float speedRatio = avgSpeed / baseSpeed;
        return (speedRatio + pathProgression) / 2f;
    }

    private float CalculateHealthThreat(float timeToKill, float maxHealth, float avgHealth)
    {
        float baseHealthThreat = avgHealth / 2500f;
        float ttk = Mathf.Min(timeToKill / 10f, 1f);
        return (baseHealthThreat + ttk) / 2f;
    }

    private float CalculateSwarmThreat(int monsterCount, float density)
    {
        return (monsterCount * 0.4f + density * 0.6f) / 2;
    }

    private float CalculateEtherealThreat(int etherealCount, int totalCount)
    {
        return etherealCount > 0 ? (float)etherealCount / totalCount : 0;
    }

    private float CalculatePathCoverageThreat(List<Monster> monsters)
    {
        var pathSegments = monsters.GroupBy(m => m.GetComponent<MonsterMovement>().currentPathNumber);
        return pathSegments.Count() / (float)GetTotalActivePaths();
    }

    private float CalculatePathCoverageForPoint(Transform point)
    {
        float totalCoverage = 0f;
        float maxRadius = Mathf.Max(GetTowerRadius(earthTowerPrefab),
                                  GetTowerRadius(lightningTowerPrefab),
                                  GetTowerRadius(fireTowerPrefab),
                                  GetTowerRadius(iceTowerPrefab));

        foreach (Transform pathTransform in pathsContainer.transform)
        {
            if (Vector3.Distance(point.position, pathTransform.position) <= maxRadius)
            {
                totalCoverage += 1f;
            }
        }

        return totalCoverage;
    }

    private List<Monster> GetActiveMonsters()
    {
        List<Monster> monsters = new List<Monster>();
        foreach (Transform child in monsterHolder.transform)
        {
            Monster monster = child.GetComponent<Monster>();
            if (monster != null)
            {
                monsters.Add(monster);
            }
        }
        return monsters;
    }

    private float CalculateTotalDPS()
    {
        float totalDPS = 0f;
        foreach (GameObject tower in activeTowers)
        {
            TowerAttackBase towerBase = tower.GetComponent<TowerAttackBase>();
            if (towerBase != null)
            {
                totalDPS += towerBase.GetDPS();
            }
        }
        return totalDPS;
    }

    private float CalculateRequiredDPS(List<Monster> monsters)
    {
        float totalHealth = monsters.Sum(m => m.GetHealth());
        float avgSpeed = monsters.Average(m => m.GetSpeed());
        return (totalHealth * avgSpeed) / 15f;
    }

    private void HandleTowerPlacement()
    {
        if (!canPerformAction) return;

        if (TryUpgradeExistingTower() || TrySpawnNewTower())
        {
            StartCoroutine(ActionCooldown());
        }
    }

    private bool TrySpawnNewTower()
    {
        GameObject towerToSpawn = DetermineBestTowerType();
        if (towerToSpawn == null) return false;

        Transform spawnPoint = FindOptimalSpawnPoint(towerToSpawn);
        if (spawnPoint == null) return false;

        SpawnTower(towerToSpawn, spawnPoint);
        return true;
    }

    private bool TryUpgradeExistingTower()
    {
        List<TowerEvaluation> evaluations = EvaluateTowerUpgrades();
        if (evaluations.Count == 0) return false;

        TowerEvaluation bestUpgrade = evaluations
            .OrderByDescending(e => e.finalScore)
            .FirstOrDefault();

        if (bestUpgrade == null) return false;

        TowerAttackBase towerBase = bestUpgrade.tower.GetComponent<TowerAttackBase>();
        int upgradeCost = towerBase.GetCost();
        int newTier = towerBase.GetTier() + 1;

        if (upgradeCost > currentMoney || newTier > 3) return false;

        towerBase.baseDamage /= statMultiplier;
        towerBase.attackCooldown *= statMultiplier;

        towerBase.SetTier(newTier);

        ModifyTowerStats(towerBase);
        UpdateTowerTierText(bestUpgrade.tower, newTier);
        currentMoney -= upgradeCost;
        return true;
    }

    private void UpdateTowerTierText(GameObject tower, int tier)
    {
        TMPro.TextMeshProUGUI tierText = tower.transform
            .Find("UI/Canvas/TierText")
            ?.GetComponent<TMPro.TextMeshProUGUI>();

        if (tierText != null)
        {
            tierText.text = tier.ToString();
        }
    }

    private float EvaluateInitialTower(GameObject towerPrefab, MonsterAnalysis analysis)
    {
        float score = 0;

        if (towerPrefab == lightningTowerPrefab)
        {
            score += analysis.totalCount * 15;
            score += analysis.hasGroupedMonsters ? 30 : 0;
            score += analysis.avgHealth < 5000 ? 40 : -20;
        }
        else if (towerPrefab == fireTowerPrefab)
        {
            score += analysis.maxHealth > 5000 ? 50 : 20;
            score += analysis.avgHealth > 2500 ? 30 : 0;
            score -= analysis.totalCount * 5;
        }
        else if (towerPrefab == iceTowerPrefab)
        {
            score += analysis.avgSpeed > 3 ? 45 : 20;
            score += analysis.totalCount * 10;
            score -= analysis.maxHealth / 1000;
        }

        return score;
    }

    private System.Type GetTowerType(GameObject tower)
    {
        if (tower == fireTowerPrefab) return typeof(FireAttack);
        if (tower == iceTowerPrefab) return typeof(IceAttack);
        if (tower == earthTowerPrefab) return typeof(EarthAttack);
        return typeof(LightningChainTower);
    }

    private List<TowerEvaluation> EvaluateTowerUpgrades()
    {
        MonsterAnalysis analysis = AnalyzeMonsters();
        List<TowerEvaluation> evaluations = new List<TowerEvaluation>();

        Dictionary<System.Type, int> upgradedTowerCounts = CountUpgradedTowerTypes();

        foreach (GameObject tower in activeTowers)
        {
            TowerAttackBase towerBase = tower.GetComponent<TowerAttackBase>();
            if (towerBase.GetTier() >= 3) continue;

            float upgradeMultiplier = 1f;
            if (tower.GetComponent<LightningChainTower>() && upgradedTowerCounts[typeof(LightningChainTower)] > 0)
            {
                upgradeMultiplier = 0.5f;
            }

            TowerEvaluation eval = new TowerEvaluation
            {
                tower = tower,
                baseScore = EvaluateBaseEffectiveness(tower, analysis) * upgradeMultiplier,
                positionScore = EvaluatePosition(tower, analysis),
                threatScore = EvaluateThreatResponse(tower, analysis),
                upgradeImpact = CalculateUpgradeImpact(tower, analysis)
            };

            eval.finalScore = CalculateFinalScore(eval, analysis);
            evaluations.Add(eval);
        }

        return evaluations;
    }

    private Dictionary<System.Type, int> CountUpgradedTowerTypes()
    {
        Dictionary<System.Type, int> counts = new Dictionary<System.Type, int>
    {
        { typeof(FireAttack), 0 },
        { typeof(IceAttack), 0 },
        { typeof(EarthAttack), 0 },
        { typeof(LightningChainTower), 0 }
    };

        foreach (GameObject tower in activeTowers)
        {
            TowerAttackBase towerBase = tower.GetComponent<TowerAttackBase>();
            if (towerBase.GetTier() > 1)
            {
                if (tower.GetComponent<FireAttack>()) counts[typeof(FireAttack)]++;
                if (tower.GetComponent<IceAttack>()) counts[typeof(IceAttack)]++;
                if (tower.GetComponent<EarthAttack>()) counts[typeof(EarthAttack)]++;
                if (tower.GetComponent<LightningChainTower>()) counts[typeof(LightningChainTower)]++;
            }
        }

        return counts;
    }

    private MonsterAnalysis AnalyzeMonsters()
    {
        List<Monster> monsters = GetActiveMonsters();

        return new MonsterAnalysis
        {
            avgHealth = monsters.Average(m => m.GetHealth()),
            avgSpeed = monsters.Average(m => m.GetSpeed()),
            etherealCount = monsters.Count(m => m.CantTarget()),
            totalCount = monsters.Count,
            maxHealth = monsters.Max(m => m.GetHealth()),
            totalHealth = monsters.Sum(m => m.GetHealth()),
            hasGroupedMonsters = CheckForGroupedMonsters(monsters),
            avgDistance = CalculateAverageMonsterDistance(monsters)
        };
    }

    private float EvaluateBaseEffectiveness(GameObject tower, MonsterAnalysis analysis)
    {
        float score = 0;

        if (tower.GetComponent<FireAttack>())
        {
            score += analysis.maxHealth * 0.5f;
            score += analysis.avgHealth * 0.3f;
        }
        else if (tower.GetComponent<IceAttack>())
        {
            score += analysis.avgSpeed * 50;
            score += analysis.totalCount * 20;
        }
        else if (tower.GetComponent<EarthAttack>())
        {
            score += analysis.hasGroupedMonsters ? 100 : 0;
            score += (analysis.totalCount - analysis.etherealCount) * 30;
        }
        else if (tower.GetComponent<LightningChainTower>())
        {
            score += analysis.totalCount * 25;
            score += analysis.hasGroupedMonsters ? 50 : 0;
        }

        return score;
    }

    private float EvaluatePosition(GameObject tower, MonsterAnalysis analysis)
    {
        float score = 0;
        Vector3 position = tower.transform.position;
        float coverageScore = GetTowerCoverageScore(tower);

        score += coverageScore * 50;

        if (tower.GetComponent<IceAttack>())
        {
            score += CountNearbyTowers(position, 3f) * 20;
        }
        else
        {
            score -= CountNearbyTowers(position, 3f) * 10;
        }

        return score;
    }

    private float EvaluateThreatResponse(GameObject tower, MonsterAnalysis analysis)
    {
        float score = 0;

        if (tower.GetComponent<FireAttack>())
        {
            score += analysis.maxHealth > 5000 ? 100 : 0;
        }
        else if (tower.GetComponent<IceAttack>())
        {
            score += analysis.avgSpeed > 3.5f ? 100 : 0;
        }
        else if (tower.GetComponent<EarthAttack>())
        {
            score += analysis.hasGroupedMonsters ? 100 : 0;
            score -= analysis.etherealCount * 20;
        }
        else if (tower.GetComponent<LightningChainTower>())
        {
            score += analysis.totalCount > 3 ? 100 : 0;
        }

        return score;
    }

    private float CalculateUpgradeImpact(GameObject tower, MonsterAnalysis analysis)
    {
        if (!towerSpawnPoints.TryGetValue(tower, out Transform spawnPoint))
        {
            return 0f;
        }

        TowerAttackBase towerBase = tower.GetComponent<TowerAttackBase>();
        float currentDPS = towerBase.GetDPS();
        float upgradeCost = towerBase.GetCost();
        float dpsPerCost = currentDPS / upgradeCost;
        float coverageScore = spawnPointPathCoverage[spawnPoint];

        return dpsPerCost * coverageScore * 100;
    }

    private float CalculateFinalScore(TowerEvaluation eval, MonsterAnalysis analysis)
    {
        float baseWeight = 0.3f;
        float positionWeight = 0.2f;
        float threatWeight = 0.3f;
        float impactWeight = 0.2f;

        return (eval.baseScore * baseWeight) +
               (eval.positionScore * positionWeight) +
               (eval.threatScore * threatWeight) +
               (eval.upgradeImpact * impactWeight);
    }

    private bool CheckForGroupedMonsters(List<Monster> monsters)
    {
        foreach (Monster monster in monsters)
        {
            int nearbyCount = monsters.Count(m =>
                Vector3.Distance(monster.transform.position, m.transform.position) < 3f);
            if (nearbyCount >= 3) return true;
        }
        return false;
    }

    private float CalculateAverageMonsterDistance(List<Monster> monsters)
    {
        if (monsters.Count == 0) return 0;

        float totalDistance = 0;
        Vector3 center = Vector3.zero;

        foreach (Monster monster in monsters)
        {
            center += monster.transform.position;
        }
        center /= monsters.Count;

        foreach (Monster monster in monsters)
        {
            totalDistance += Vector3.Distance(monster.transform.position, center);
        }

        return totalDistance / monsters.Count;
    }

    private int CountNearbyTowers(Vector3 position, float radius)
    {
        return activeTowers.Count(t =>
            Vector3.Distance(t.transform.position, position) <= radius);
    }

    private GameObject DetermineBestTowerType()
    {
        if (activeTowers.Count == 0)
        {
            var monsters = GetActiveMonsters();
            if (monsters.Count == 0) return null;

            var monsterAnalysis = AnalyzeMonsters();
            GameObject bestStartingTower = null;
            float bestScore = float.MinValue;

            foreach (var towerPrefab in new[] { lightningTowerPrefab, fireTowerPrefab })
            {
                if (GetTowerCost(towerPrefab) > currentMoney) continue;

                float score = EvaluateInitialTower(towerPrefab, monsterAnalysis);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestStartingTower = towerPrefab;
                }
            }

            return bestStartingTower;
        }

        List<Monster> activeMonsters = GetActiveMonsters();
        if (activeMonsters.Count == 0) return null;

        ThreatAssessment threats = AnalyzeThreats(activeMonsters, CalculateTotalDPS());
        Dictionary<System.Type, int> towerCounts = CountExistingTowerTypes();

        var towerScores = new Dictionary<GameObject, float>();
        foreach (var tower in new[] { iceTowerPrefab, fireTowerPrefab, earthTowerPrefab, lightningTowerPrefab })
        {
            if (GetTowerCost(tower) > currentMoney) continue;
            float baseScore = EvaluateTowerNeed(tower, threats, towerCounts);
            towerScores[tower] = baseScore;
        }

        return towerScores.Count > 0 ? towerScores.OrderByDescending(kvp => kvp.Value).First().Key : null;
    }

    private float EvaluateTowerNeed(GameObject tower, ThreatAssessment threats, Dictionary<System.Type, int> counts)
    {
        System.Type towerType = GetTowerType(tower);
        int existingCount = counts[towerType];
        float maxThreat = Mathf.Max(threats.swarmThreat, threats.healthThreat, threats.speedThreat);
        if (maxThreat == 0) maxThreat = 1;

        float normalizedHealth = threats.healthThreat / maxThreat;
        float normalizedSpeed = threats.speedThreat / maxThreat;
        float normalizedSwarm = threats.swarmThreat / maxThreat;

        float baseScore = 0;
        float diversityMultiplier = CalculateDiversityMultiplier(towerType, counts);

        if (towerType == typeof(LightningChainTower))
        {
            baseScore = normalizedSwarm * 0.7f;
            if (existingCount > 0) baseScore *= 0.7f;
        }
        else if (towerType == typeof(IceAttack))
        {
            if (counts[typeof(FireAttack)] + counts[typeof(LightningChainTower)] + counts[typeof(EarthAttack)] == 0) return 0;

            baseScore = normalizedSpeed * 0.8f;
            float supportMultiplier = 1f + (counts[typeof(FireAttack)] * 0.3f +
                                          counts[typeof(LightningChainTower)] * 0.2f +
                                          counts[typeof(EarthAttack)] * 0.3f);
            baseScore *= supportMultiplier;
        }
        else if (towerType == typeof(FireAttack))
        {
            baseScore = normalizedHealth * 0.65f;
            if (normalizedHealth > 0.3f && existingCount == 0) baseScore *= 1.2f;
        }
        else if (towerType == typeof(EarthAttack))
        {
            if (threats.etherealThreat > 0.4f) return 0;
            baseScore = (normalizedSwarm * 0.6f + normalizedSpeed * 0.4f);
            if (counts[typeof(IceAttack)] > 0) baseScore *= 1.2f;
        }

        int totalTowers = counts.Values.Sum();
        if (totalTowers > 0)
        {
            float proportion = (float)counts[towerType] / totalTowers;
            if (proportion >= 0.33f) baseScore *= 0.6f;
        }

        return ApplyDiminishingReturns(baseScore * diversityMultiplier, existingCount);
    }

    private float CalculateDiversityMultiplier(System.Type towerType, Dictionary<System.Type, int> counts)
    {
        int totalTowers = counts.Values.Sum();
        if (totalTowers == 0) return 1f;

        float proportion = (float)counts[towerType] / totalTowers;
        float baseMultiplier = counts[towerType] == 0 ? 1.2f : 1f;

        if (proportion < 0.2f) return baseMultiplier * 1.2f;
        if (proportion > 0.33f) return 0.7f;

        return baseMultiplier;
    }

    private float NormalizeTowerScore(float score, System.Type towerType, Dictionary<System.Type, int> counts)
    {
        int totalTowers = counts.Values.Sum();
        if (totalTowers == 0) return score;

        float maxAllowedProportion = 0.35f;
        float currentProportion = (float)counts[towerType] / totalTowers;

        if (currentProportion >= maxAllowedProportion)
        {
            score *= 0.5f;
        }

        return score;
    }

    private int GetTowerCost(GameObject towerPrefab)
    {
        return towerPrefab.GetComponent<TowerAttackBase>().GetCost();
    }

    private float EvaluateIceTowerNeed(ThreatAssessment threats, Dictionary<System.Type, int> counts)
    {
        float score = threats.speedThreat * 2 + threats.pathCoverageThreat;
        return ApplyDiminishingReturns(score, counts[typeof(IceAttack)]);
    }

    private float EvaluateFireTowerNeed(ThreatAssessment threats, Dictionary<System.Type, int> counts)
    {
        float score = threats.healthThreat * 1.5f + threats.pathCoverageThreat * 0.5f;
        return ApplyDiminishingReturns(score, counts[typeof(FireAttack)]);
    }

    private float EvaluateEarthTowerNeed(ThreatAssessment threats, Dictionary<System.Type, int> counts)
    {
        if (threats.etherealThreat > 0.4f) return 0;
        float score = threats.swarmThreat * 2 + threats.pathCoverageThreat;
        return ApplyDiminishingReturns(score, counts[typeof(EarthAttack)]);
    }

    private float EvaluateLightningTowerNeed(ThreatAssessment threats, Dictionary<System.Type, int> counts)
    {
        float score = (threats.swarmThreat + threats.healthThreat + threats.speedThreat) / 3;
        return ApplyDiminishingReturns(score, counts[typeof(LightningChainTower)]);
    }

    private float ApplyDiminishingReturns(float score, int existingCount)
    {
        return score * Mathf.Pow(0.7f, existingCount);
    }

    private float CalculateMonsterDensity(List<Monster> monsters)
    {
        if (monsters.Count <= 1) return 0;

        float totalDistance = 0;
        int connections = 0;

        for (int i = 0; i < monsters.Count; i++)
        {
            for (int j = i + 1; j < monsters.Count; j++)
            {
                totalDistance += Vector3.Distance(
                    monsters[i].transform.position,
                    monsters[j].transform.position
                );
                connections++;
            }
        }

        return connections > 0 ? monsters.Count / (totalDistance / connections) : 0;
    }

    private float CalculatePathProgression(List<Monster> monsters)
    {
        float avgPathProgress = monsters.Average(m =>
        {
            var movement = m.GetComponent<MonsterMovement>();
            return (float)movement.currentPathNumber / movement.endPathNumber;
        });

        return avgPathProgress;
    }

    private int GetTotalActivePaths()
    {
        return pathsContainer.transform.childCount;
    }

    private Dictionary<System.Type, int> CountExistingTowerTypes()
    {
        Dictionary<System.Type, int> counts = new Dictionary<System.Type, int>
        {
            { typeof(FireAttack), 0 },
            { typeof(IceAttack), 0 },
            { typeof(EarthAttack), 0 },
            { typeof(LightningChainTower), 0 }
        };

        foreach (GameObject tower in activeTowers)
        {
            if (tower.GetComponent<FireAttack>()) counts[typeof(FireAttack)]++;
            if (tower.GetComponent<IceAttack>()) counts[typeof(IceAttack)]++;
            if (tower.GetComponent<EarthAttack>()) counts[typeof(EarthAttack)]++;
            if (tower.GetComponent<LightningChainTower>()) counts[typeof(LightningChainTower)]++;
        }

        return counts;
    }

    private float GetTowerCoverageScore(GameObject tower)
    {
        return towerSpawnPoints.TryGetValue(tower, out Transform spawnPoint)
            ? spawnPointPathCoverage[spawnPoint]
            : 0f;
    }

    private Transform FindOptimalSpawnPoint(GameObject towerPrefab)
    {
        float towerRadius = GetTowerRadius(towerPrefab);
        Transform bestPoint = null;
        float bestScore = float.MinValue;

        foreach (Transform point in spawnHolder.transform)
        {
            if (IsSpawnPointOccupied(point)) continue;

            float coverageScore = spawnPointPathCoverage[point];
            float proximityPenalty = CalculateProximityPenalty(point);
            float finalScore = coverageScore - proximityPenalty;

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private float GetTowerRadius(GameObject towerPrefab)
    {
        TowerAttackBase towerBase = towerPrefab.GetComponent<TowerAttackBase>();
        return towerBase != null ? towerBase.towerRadius : 5f;
    }

    private bool IsSpawnPointOccupied(Transform point)
    {
        return activeTowers.Any(t => Vector3.Distance(t.transform.position, point.position) < 1f);
    }

    private float CalculateProximityPenalty(Transform point)
    {
        float penalty = 0f;
        foreach (GameObject tower in activeTowers)
        {
            float distance = Vector3.Distance(tower.transform.position, point.position);
            if (distance < 5f)
            {
                penalty += (5f - distance) * 0.2f;
            }
        }
        return penalty;
    }

    private void ModifyTowerStats(TowerAttackBase tower)
    {
        tower.baseDamage *= statMultiplier;
        tower.attackCooldown /= statMultiplier;
    }

    private void SpawnTower(GameObject towerPrefab, Transform spawnPoint)
    {
        if (occupiedSpawnPoints.Contains(spawnPoint) || currentMoney < GetTowerCost(towerPrefab)) return;

        float heightOffset = DetermineTowerHeight(towerPrefab);
        Vector3 spawnPosition = spawnPoint.position + new Vector3(0, heightOffset, 0);

        GameObject tower = Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
        ModifyTowerStats(tower.GetComponent<TowerAttackBase>());

        activeTowers.Add(tower);
        towerSpawnPoints[tower] = spawnPoint;
        occupiedSpawnPoints.Add(spawnPoint);

        currentMoney -= GetTowerCost(towerPrefab);
    }


    private IEnumerator SpawnCooldown()
    {
        canSpawn = false;
        yield return new WaitForSeconds(towerSpawnCooldown);
        canSpawn = true;
    }

    private float DetermineTowerHeight(GameObject towerPrefab)
    {
        if (towerPrefab == earthTowerPrefab)
        {
            return 1f;
        }
        return 1.5f;
    }

    private void OnDestroy()
    {
        towerSpawnPoints.Clear();
        spawnPointPathCoverage.Clear();
        activeTowers.Clear();
        occupiedSpawnPoints.Clear();
    }
    private IEnumerator ActionCooldown()
    {
        canPerformAction = false;
        yield return new WaitForSeconds(towerSpawnCooldown);
        canPerformAction = true;
    }
}