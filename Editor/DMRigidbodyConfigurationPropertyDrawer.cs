using DeterministicPhysicsLibrary.Runtime;
using DeterministicPhysicsLibrary.Unity;
using UnityEditor;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Editor
{
    [CustomPropertyDrawer(typeof(DRigidbodyConfiguration))]
    public class DMRigidbodyConfigurationPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty dynamicsProperty = property.FindPropertyRelative("dynamics");
            SerializedProperty collisionDetectionProperty = property.FindPropertyRelative("collisionDetection");
            SerializedProperty collisionResponseProperty = property.FindPropertyRelative("collisionResponse");

            SerializedProperty colliderTypeProperty = collisionDetectionProperty.FindPropertyRelative("colliderType");

            EditorGUILayout.PropertyField(dynamicsProperty);
            EditorGUILayout.PropertyField(collisionDetectionProperty);

            if (colliderTypeProperty.enumValueIndex != (int)ColliderType.None)
            {
                EditorGUILayout.PropertyField(collisionResponseProperty);
            }
        }
    }
}