using ChroniclesOfRus.CameraSystem;
using ChroniclesOfRus.Characters.Player;
using ChroniclesOfRus.Characters.Player.StateMachine;
using ChroniclesOfRus.Characters.Enemy;
using ChroniclesOfRus.Characters.Enemy.StateMachine;
using ChroniclesOfRus.Combat;
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
            CreateDamageableDummy();
            CreateDamageTestTrigger();
            CreateBasicEnemy(player.transform);
            Transform cameraTransform = CreateCamera(player.transform);
            AssignPlayerCamera(player, cameraTransform);
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
            player.AddComponent<HealthComponent>();
            player.AddComponent<PlayerDamageReceiver>();
            player.AddComponent<PlayerAnimationController>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.up;

            return player;
        }

        private static void CreateDamageableDummy()
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = "Damageable Dummy";
            dummy.transform.position = new Vector3(0f, 1f, 1.7f);
            dummy.AddComponent<HealthComponent>();
            dummy.AddComponent<DamageableDummy>();
        }

        private static void CreateDamageTestTrigger()
        {
            GameObject triggerObject = new GameObject("Damage Test Trigger");
            triggerObject.transform.position = new Vector3(-3f, 0.5f, 0f);

            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.5f, 1f, 1.5f);

            Rigidbody rigidbody = triggerObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            triggerObject.AddComponent<DamageTestTrigger>();
        }

        private static void CreateBasicEnemy(Transform player)
        {
            GameObject enemy = new GameObject("Basic Enemy");
            enemy.transform.position = new Vector3(0f, 0f, 6f);

            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;

            HealthComponent health = enemy.AddComponent<HealthComponent>();
            EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
            EnemyDetection detection = enemy.AddComponent<EnemyDetection>();
            EnemyCombat combat = enemy.AddComponent<EnemyCombat>();
            EnemyStateMachine stateMachine = enemy.AddComponent<EnemyStateMachine>();
            EnemyAnimationController animationController = enemy.AddComponent<EnemyAnimationController>();
            EnemyController enemyController = enemy.AddComponent<EnemyController>();

            SerializedObject detectionObject = new SerializedObject(detection);
            detectionObject.FindProperty("target").objectReferenceValue = player;
            detectionObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject controllerObject = new SerializedObject(enemyController);
            controllerObject.FindProperty("target").objectReferenceValue = player;
            controllerObject.FindProperty("characterController").objectReferenceValue = controller;
            controllerObject.FindProperty("health").objectReferenceValue = health;
            controllerObject.FindProperty("movement").objectReferenceValue = movement;
            controllerObject.FindProperty("detection").objectReferenceValue = detection;
            controllerObject.FindProperty("combat").objectReferenceValue = combat;
            controllerObject.FindProperty("stateMachine").objectReferenceValue = stateMachine;
            controllerObject.FindProperty("animationController").objectReferenceValue = animationController;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            visual.transform.SetParent(enemy.transform, false);
            visual.transform.localPosition = Vector3.up;
        }

        private static Transform CreateCamera(Transform target)
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
            return cameraObject.transform;
        }

        private static void AssignPlayerCamera(GameObject player, Transform cameraTransform)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            SerializedObject movementObject = new SerializedObject(movement);
            movementObject.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
            movementObject.ApplyModifiedPropertiesWithoutUndo();
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
