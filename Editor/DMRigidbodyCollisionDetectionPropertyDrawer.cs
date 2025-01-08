using DeterministicPhysicsLibrary.Runtime;
using DeterministicPhysicsLibrary.Unity;
using UnityEditor;
using UnityEngine;

namespace DeterministicPhysicsLibrary.Editor
{
    [CustomPropertyDrawer(typeof(DRigidbodyCollisionDetection))]
    public class DMRigidbodyCollisionDetectionPropertyDrawer : PropertyDrawer
    {
        bool foldoutOpen = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, label);

            if (foldoutOpen) 
            {
                SerializedProperty colliderTypeProperty = property.FindPropertyRelative("colliderType");
                SerializedProperty collisionLayerProperty = property.FindPropertyRelative("collisionLayer");
                SerializedProperty extentsProperty = property.FindPropertyRelative("extents");
                SerializedProperty radiusProperty = property.FindPropertyRelative("radius");

                EditorGUILayout.PropertyField(colliderTypeProperty);

                switch ((ColliderType)colliderTypeProperty.enumValueIndex)
                {
                    case ColliderType.None:
                        break;
                    case ColliderType.Box:
                        EditorGUILayout.PropertyField(collisionLayerProperty);
                        EditorGUILayout.PropertyField(extentsProperty);
                        break;
                    case ColliderType.Sphere:
                        EditorGUILayout.PropertyField(collisionLayerProperty);
                        EditorGUILayout.PropertyField(radiusProperty);
                        break;
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}