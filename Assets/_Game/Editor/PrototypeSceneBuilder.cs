using ChroniclesOfRus.CameraSystem;
using ChroniclesOfRus.Characters.Player;
using ChroniclesOfRus.Characters.Player.StateMachine;
using ChroniclesOfRus.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ChroniclesOfRus.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototype_PlayerMovement.unity";
        private const string InputPath = "Assets/_Game/Input/PlayerControls.inputactions";

        [MenuItem("Chronicles of Rus/Build Prototype Scene")]
        public static void Build()
        {
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (inputAsset == null)
                throw new UnityException($"Input Action Asset not found at {InputPath}");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateArena();
            GameObject player = CreatePlayer(inputAsset);
            CreateCamera(player.transform);
            CreateLight();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Selection.activeGameObject = player;
            Debug.Log($"Prototype scene created: {ScenePath}");
        }

        private static void CreateArena()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Arena";
            floor.transform.SetPositionAndRotation(new Vector3(0f, -0.5f, 0f), Quaternion.identity);
            floor.transform.localScale = new Vector3(20f, 1f, 20f);

            CreateObstacle(new Vector3(3f, 1f, 2f), new Vector3(2f, 2f, 2f));
            CreateObstacle(new Vector3(-3f, 0.75f, 3f), new Vector3(1.5f, 1.5f, 4f));
            CreateObstacle(new Vector3(1f, 0.5f, -4f), new Vector3(4f, 1f, 1.5f));
        }

        private static void CreateObstacle(Vector3 position, Vector3 scale)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle";
            obstacle.transform.SetPositionAndRotation(position, Quaternion.identity);
            obstacle.transform.localScale = scale;
        }

        private static GameObject CreatePlayer(InputActionAsset inputAsset)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = Vector3.zero;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;

            PlayerInputReader reader = player.AddComponent<PlayerInputReader>();
            SerializedObject readerObject = new SerializedObject(reader);
            readerObject.FindProperty("inputActions").objectReferenceValue = inputAsset;
            readerObject.ApplyModifiedPropertiesWithoutUndo();

            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerStateMachine>();
            player.AddComponent<PlayerAnimationController>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.up;

            return player;
        }

        private static void CreateCamera(Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            cameraObject.AddComponent<AudioListener>();

            IsometricCameraController controller = cameraObject.AddComponent<IsometricCameraController>();
            SerializedObject cameraControllerObject = new SerializedObject(controller);
            cameraControllerObject.FindProperty("target").objectReferenceValue = target;
            cameraControllerObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void AddSceneToBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
