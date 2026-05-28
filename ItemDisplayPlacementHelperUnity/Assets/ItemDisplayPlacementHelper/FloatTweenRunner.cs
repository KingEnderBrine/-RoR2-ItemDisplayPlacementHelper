using System.Collections;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    internal class FloatTweenRunner
    {
        protected MonoBehaviour m_CoroutineContainer;
        protected IEnumerator m_Tween;

        private static IEnumerator Start(FloatTween tweenInfo)
        {
            if (tweenInfo.ValidTarget())
            {
                float elapsedTime = 0.0f;
                while ((double)elapsedTime < (double)tweenInfo.duration)
                {
                    elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                    tweenInfo.TweenValue(Mathf.Clamp01(elapsedTime / tweenInfo.duration));
                    yield return (object)null;
                }
                tweenInfo.TweenValue(1f);
            }
        }

        public void Init(MonoBehaviour coroutineContainer) => this.m_CoroutineContainer = coroutineContainer;

        public void StartTween(FloatTween info)
        {
            if (this.m_CoroutineContainer == null)
            {
                Debug.LogWarning((object)"Coroutine container not configured... did you forget to call Init?");
            }
            else
            {
                this.StopTween();
                if (!this.m_CoroutineContainer.gameObject.activeInHierarchy)
                {
                    info.TweenValue(1f);
                }
                else
                {
                    this.m_Tween = FloatTweenRunner.Start(info);
                    this.m_CoroutineContainer.StartCoroutine(this.m_Tween);
                }
            }
        }

        public void StopTween()
        {
            if (this.m_Tween == null)
                return;
            this.m_CoroutineContainer.StopCoroutine(this.m_Tween);
            this.m_Tween = (IEnumerator)null;
        }
    }
}
