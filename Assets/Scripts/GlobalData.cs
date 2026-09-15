public class GlobalData
{
    private static GlobalData instance = null;

    public static GlobalData SharedInstance
    {
        get
        {
            if (instance == null)
                instance = new GlobalData();
            return instance;
        }
    }

    public int nextSceneIndex = 1;

    public float[] levelTimes = new float[3] { 0f, 0f, 0f };

    public int unlockedLevels = 1;
}