using UnityEngine;

/// <summary>
/// Traspaso del nivel elegido entre el menu y la escena de juego. Se rellena
/// justo antes de SceneManager.LoadScene y lo lee LevelManager al arrancar.
/// </summary>
public static class LevelSession
{
    public static LevelData Requested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        Requested = null;
    }
}
