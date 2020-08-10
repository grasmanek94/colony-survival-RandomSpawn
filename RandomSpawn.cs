using Pipliz.JSON;
using System.Collections.Generic;
using System.IO;
using TerrainGeneration;
using Pipliz;

namespace grasmanek94.RandomSpawn
{
    [ModLoader.ModManager]
    public static class RandomSpawn
    {
        public static string config_file;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "grasmanek94.RandomSpawn.AfterWorldLoad")]
        static void AfterWorldLoad()
        {
            config_file = Path.Combine("gamedata", "savegames", ServerManager.WorldName, "random_spawn_config.json");

            TerrainGenerator terrainGenerator = ServerManager.TerrainGenerator as TerrainGenerator;
            RandomSpawnProvider spawn_provider = new RandomSpawnProvider();

            bool not_exists = !File.Exists(config_file);
            if (not_exists)
            {
                JSON.Serialize(config_file, JSON.DeserializeString("{\"coarsness\": 10,\"ranges\": [{\"min\": [-100, -100],\"max\": [100, 100]},{\"min\": [-1000, -1000],\"max\": [-900, -900]}],\"spawnpoints\": [[0, 0],[10, 10],[-10,-10]]}"));
            }

            var data = JSON.Deserialize(config_file, false);

            data.TryGetAsOrDefault("coarsness", out spawn_provider.coarsness, 10);
            if (spawn_provider.coarsness < 1)
            {
                Log.WriteWarning("RandomSpawn: 'coarsness' must be at least 1, it's currently set to '{0}', setting to '1' automatically.", spawn_provider.coarsness);
                spawn_provider.coarsness = 1;
            }

            JSONNode ranges;
            if (data.TryGetChild("ranges", out ranges))
            {
                foreach (var range in ranges.LoopArray())
                {
                    JSONNode range_min;
                    JSONNode range_max;

                    if (range.TryGetAs("min", out range_min) && range.TryGetAs("max", out range_max))
                    {
                        Vector2Int a = new Vector2Int(range_min[0].GetAs<int>(), range_min[1].GetAs<int>());
                        Vector2Int b = new Vector2Int(range_max[0].GetAs<int>(), range_max[1].GetAs<int>());

                        spawn_provider.ranges.Add(
                            new KeyValuePair<Vector2Int, Vector2Int>(
                                new Vector2Int(Math.Min(a.x, b.x), Math.Min(a.y, b.y)),
                                new Vector2Int(Math.Max(a.x, b.x), Math.Max(a.y, b.y))
                            )
                        );
                    }
                }
            }

            JSONNode spawnpoints;
            if (data.TryGetChild("spawnpoints", out spawnpoints))
            {
                foreach (var spawnpoint in spawnpoints.LoopArray())
                {
                    if (spawnpoint.NodeType == NodeType.Array && spawnpoint.ChildCount == 2)
                    {
                        spawn_provider.custom_spawnpoints.Add(new Vector2Int(spawnpoint[0].GetAs<int>(), spawnpoint[1].GetAs<int>()));
                    }
                }
            }

            if (not_exists)
            {
                Log.WriteWarning("RandomSpawn: File \"{0}\" doesn't exist and has been created. Using defaults. Please edit this file.", config_file);
            }

            terrainGenerator.SpawnPointProvider = spawn_provider;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRespawn, "grasmanek94.RandomSpawn.OnPlayerRespawn")]
        static void OnPlayerRespawn(Players.Player player)
        {
            ServerManager.WorldSettingsVariable.SpawnPosition = (ServerManager.TerrainGenerator as TerrainGenerator).SpawnPointProvider.GetSpawnPoint();
        }
        
    }
}
