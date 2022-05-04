using System.Text;
using UnityEditor;

namespace Housewolf.EntitySystem
{
    [CustomEditor(typeof(EntitySystemContainer))]
    public class EntitySystemContainerEditor : HswlfBaseEditor<EntitySystemContainer>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!_target)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (var manager in _target.Managers)
            {
                sb.AppendLine($"{manager.Key} - {manager.Value.EntityCount} entities");
            }

            EditorGUILayout.LabelField(sb.ToString());

            Repaint();
        }
    }
}
