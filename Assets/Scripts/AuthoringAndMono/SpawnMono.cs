using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace StaticMeshes
{
    [ExecuteAlways]
    public class SpawnMono : MonoBehaviour
    {
        [SerializeField]
        private SpawnerSettings _spawnerSettings;

        private World World => World.DefaultGameObjectInjectionWorld;
        private SpawnSystem SpawnSystem => World.GetOrCreateSystemManaged<SpawnSystem>();

        private void OnEnable()
        {
            StartCoroutine(WaitUntillWorldIsCreatedAndInitializeSpawning());
        }

        // Delay spawn system update until World is created.
        // Trying to acces it in on Enable sometimes couses problems as there is no clear rule
        // whether OnEnable or World Creation happens sooner.
        private IEnumerator WaitUntillWorldIsCreatedAndInitializeSpawning()
        {
            if (World == null)
            {
                yield return new WaitForEndOfFrame();
            }
            OnSettingsChanged();
            _spawnerSettings.OnAnyValueChanged += OnSettingsChanged;
        }

        private void OnDisable()
        {
            _spawnerSettings.OnAnyValueChanged -= OnSettingsChanged;
        }

        private void OnSettingsChanged()
        {
            SpawnSystem.UpdateSettingsAndView(_spawnerSettings);
        }
    }
}