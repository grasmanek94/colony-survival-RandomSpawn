using Pipliz;
using System.Collections.Generic;
using TerrainGeneration;
using static TerrainGeneration.TerrainGenerator;

namespace grasmanek94.RandomSpawn
{
    public class RandomSpawnProvider : ISpawnPointProvider
    {
        public static readonly int max_retries = 50;
        public int coarsness;
        public List<KeyValuePair<Vector2Int, Vector2Int>> ranges;
        public List<Vector2Int> custom_spawnpoints;

        public RandomSpawnProvider()
        {
            coarsness = 10;
            ranges = new List<KeyValuePair<Vector2Int, Vector2Int>>();
            custom_spawnpoints = new List<Vector2Int>();
        }

        public Vector3Int GetSpawnPoint()
        {
            TerrainGenerator terrainGenerator = ServerManager.TerrainGenerator as TerrainGenerator;
            int waterLevel = terrainGenerator.GeneratorSettings.BaseSettings.WaterLevel;

            bool use_range = ranges.Count > 0;
            bool use_custom = custom_spawnpoints.Count > 0;
            bool use_both = use_range && use_custom;

            if (use_both)
            {
                if (Random.Next() % 2 == 0)
                {
                    use_range = false;
                }
                else
                {
                    use_custom = false;
                }
            }

            if (use_range)
            {
                for (int i = 0; i < max_retries; ++i)
                {
                    KeyValuePair<Vector2Int, Vector2Int> range = ranges[Random.Next() % ranges.Count];

                    Vector2Int delta = (range.Value - range.Key) / coarsness;
                    Vector2Int rand = new Vector2Int(Random.Next() % coarsness, Random.Next() % coarsness);
                    Vector2Int result = range.Key + new Vector2Int(delta.x * rand.x, delta.y * rand.y);

                    int num = (int)terrainGenerator.QueryData(result.x, result.y).Height;
                    if (num > waterLevel)
                    {
                        //Log.WriteWarning("GetSpawnPoint::use_range({0}, {1}, {2})", result.x, num + 1, result.y);
                        return new Vector3Int(result.x, num + 1, result.y);
                    }
                }

                use_custom = custom_spawnpoints.Count > 0;
            }

            if (use_custom)
            {
                Vector2Int result = custom_spawnpoints[Random.Next() % custom_spawnpoints.Count];
                int num = (int)terrainGenerator.QueryData(result.x, result.y).Height;
                //Log.WriteWarning("GetSpawnPoint::use_custom({0}, {1}, {2})", result.x, num + 1, result.y);
                return new Vector3Int(result.x, num + 1, result.y);
            }

            //Log.WriteWarning("GetSpawnPoint::default()");
            return new Vector3Int(0, (int)terrainGenerator.QueryData(0, 0).Height + 1, 0);
        }
    }
}
