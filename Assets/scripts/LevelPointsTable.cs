using UnityEngine;

[CreateAssetMenu(menuName = "Game/Scoring/Level Points Table")]
public class LevelPointsTable : ScriptableObject
{
    [Tooltip("pointsByLevel[1] -> level 1 points, pointsByLevel[0] is unused.")]
    public int[] pointsByLevel = new int[] { 0, 1, 3, 5, 7, 10, 15 };

    public int GetPoints(int level)
    {
        if (pointsByLevel == null || pointsByLevel.Length == 0)
            return 0;
        if (level < 0)
            level = 0;
        if (level >= pointsByLevel.Length)
            level = pointsByLevel.Length - 1;
        return pointsByLevel[level];
    }
}
