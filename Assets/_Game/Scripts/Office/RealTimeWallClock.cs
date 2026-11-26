using System;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Animates real-time 3D Hour, Minute, and Red Second hands on the Office Wall Clock
    /// synced to the player's actual local system clock!
    /// </summary>
    public class RealTimeWallClock : MonoBehaviour
    {
        [SerializeField] private Transform hourHand;
        [SerializeField] private Transform minuteHand;
        [SerializeField] private Transform secondHand;

        public void Configure(Transform hour, Transform minute, Transform second)
        {
            hourHand = hour;
            minuteHand = minute;
            secondHand = second;
        }

        private void Update()
        {
            DateTime now = DateTime.Now;
            float secAngle = now.Second * 6f;
            float minAngle = (now.Minute + now.Second / 60f) * 6f;
            float hourAngle = ((now.Hour % 12) + now.Minute / 60f) * 30f;

            // Clock face points along local +X, so hands rotate clockwise around +X!
            if (hourHand != null) hourHand.localRotation = Quaternion.Euler(hourAngle, 0f, 0f);
            if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(minAngle, 0f, 0f);
            if (secondHand != null) secondHand.localRotation = Quaternion.Euler(secAngle, 0f, 0f);
        }
    }
}
