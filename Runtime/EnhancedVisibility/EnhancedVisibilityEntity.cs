using Housewolf.EntitySystem;
using UnityEngine;

namespace Housewolf
{
    /// <summary>
    /// Attach to any GameObject to increase visibility at a distance. This helps
    /// compensate for relatively low resolutions of VR headsets to make an object
    /// more visible at distances a real pilot would be able to see them.
    /// see: https://why485.itch.io/smart-scaling-demonstration
    /// </summary>
    public class EnhancedVisibilityEntity : MonoBehaviour, IEntity
    {
        private int _index;

        public int Index => _index;

        private void Start()
        {
            _index = EntitySystemContainer.Current.GetManager<EnhancedVisibilityManager>().Register(this);
        }
    }
}
