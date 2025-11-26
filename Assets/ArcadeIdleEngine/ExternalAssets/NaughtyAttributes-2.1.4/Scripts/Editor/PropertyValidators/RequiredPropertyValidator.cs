using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.ValidatorAttributes;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Editor.Utility;
using UnityEditor;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Editor.PropertyValidators
{
    public class RequiredPropertyValidator : PropertyValidatorBase
    {
        public override void ValidateProperty(SerializedProperty property)
        {
            RequiredAttribute requiredAttribute = PropertyUtility.GetAttribute<RequiredAttribute>(property);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    string errorMessage = property.name + " is required";
                    if (!string.IsNullOrEmpty(requiredAttribute.Message))
                    {
                        errorMessage = requiredAttribute.Message;
                    }

                    NaughtyEditorGUI.HelpBox_Layout(errorMessage, MessageType.Error, context: property.serializedObject.targetObject);
                }
            }
            else
            {
                string warning = requiredAttribute.GetType().Name + " works only on reference types";
                NaughtyEditorGUI.HelpBox_Layout(warning, MessageType.Warning, context: property.serializedObject.targetObject);
            }
        }
    }
}
