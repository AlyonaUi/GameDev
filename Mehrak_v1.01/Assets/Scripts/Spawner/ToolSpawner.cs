using System.Collections;
using UnityEngine;

public class ToolSpawner : MonoBehaviour
{
    [Header("Fallbacks (optional)")]
    public ToolFactory factory;     
    public GameConfigSO config;     

    private IToolPool pool;
    private IToolFactory factoryInterface;
    private IPlayer player;
    private IGameConfig gameConfig;

    private void Start()
    {
        pool = ServiceLocator.Get<IToolPool>();
        factoryInterface = ServiceLocator.Get<IToolFactory>();
        player = ServiceLocator.Get<IPlayer>();
        gameConfig = ServiceLocator.Get<IGameConfig>();
        
        if (factoryInterface == null && factory != null) factoryInterface = factory;
        if (gameConfig == null && config != null) gameConfig = config;
        if (player == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                var pComp = playerGO.GetComponent<PlayerController>();
                if (pComp != null) player = pComp;
            }
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        var interval = (gameConfig != null) ? gameConfig.SpawnInterval : (config != null ? config.spawnInterval : 1f);
        while (true)
        {
            TrySpawnAllTypes();
            yield return new WaitForSeconds(interval);
        }
    }

    private void TrySpawnAllTypes()
    {
        var cfg = gameConfig ?? (config as IGameConfig);
        if (cfg == null) return;

        foreach (ToolType type in System.Enum.GetValues(typeof(ToolType)))
        {
            var data = GetDataForType(type);
            if (data == null) continue;

            if (pool != null && pool.ActiveCount(type) >= data.maxCount) continue;

            var pos = FindSpawnPosition(cfg);
            if (pos.HasValue)
            {
                if (IsTooCloseToTools(pos.Value, cfg.MinToolDistance)) continue;
                if (player != null && Vector2.Distance(pos.Value, player.Position) < cfg.MinDistanceFromPlayer) continue;

                if (factoryInterface != null)
                    factoryInterface.Spawn(type, pos.Value);
                else if (factory != null)
                    factory.Spawn(type, pos.Value);
            }
        }
    }

    private ToolDataSO GetDataForType(ToolType type)
    {
        if (factory != null)
        {
            foreach (var e in factory.entries)
                if (e.toolType == type) return e.data;
        }
        else
        {
            var f = ServiceLocator.Get<IToolFactory>() as ToolFactory;
            if (f != null)
            {
                foreach (var e in f.entries)
                    if (e.toolType == type) return e.data;
            }
        }
        return null;
    }

    private Vector2? FindSpawnPosition(IGameConfig cfg)
    {
        int tries = 10;
        for (int i = 0; i < tries; i++)
        {
            var half = cfg.SpawnAreaSize * 0.5f;
            var x = Random.Range(-half.x, half.x) + cfg.SpawnAreaCenter.x;
            var y = Random.Range(-half.y, half.y) + cfg.SpawnAreaCenter.y;
            var pos = new Vector2(x, y);
            return pos;
        }
        return null;
    }

    private bool IsTooCloseToTools(Vector2 pos, float minDist)
    {
        if (pool == null) return false;
        foreach (var t in pool.GetActiveTools())
        {
            if (t == null) continue;
            if (Vector2.Distance(pos, t.transform.position) < minDist) return true;
        }
        return false;
    }
}