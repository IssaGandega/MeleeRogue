using System.Collections;
using UnityEngine;

namespace Managers
{
    // Flash de sprite partagé, possédé par le script qui en a besoin.
    // Même principe que KnockbackHandler/CameraShake : une classe simple qui
    // lance sa coroutine sur le MonoBehaviour qu'on lui donne, au lieu de
    // dupliquer la même logique dans chaque script.
    public class HitFlash
    {
        private readonly MonoBehaviour _owner;
        private readonly SpriteRenderer _spriteRenderer;
        private readonly Color _defaultColor;
        private Coroutine _active;

        public HitFlash(MonoBehaviour owner, SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
                Debug.LogError("HitFlash: spriteRenderer not found (null). Hit feedback will be skipped.", owner);

            _owner = owner;
            _spriteRenderer = spriteRenderer;
            _defaultColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        }

        public void Flash(Color color, float duration)
        {
            if (_spriteRenderer == null) return;

            if (_active != null)
                _owner.StopCoroutine(_active);
            _active = _owner.StartCoroutine(Run(color, duration));
        }

        private IEnumerator Run(Color color, float duration)
        {
            _spriteRenderer.color = color;
            yield return new WaitForSecondsRealtime(duration);
            _spriteRenderer.color = _defaultColor;
        }
    }
}
