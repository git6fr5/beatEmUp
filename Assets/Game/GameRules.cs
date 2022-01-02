// Libraries.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour {

    [System.Serializable]
    public struct RandomParams {

        public float mid => (min + max) / 2f;
        public float range => max - min;

        public float fValue => Random.Range(min, max);
        public int iValue => (int)Mathf.Round(Random.Range(min, max));

        public float min;
        public float max;

        public RandomParams(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public float Evaluate(float ratio) {
            ratio = ratio > 1 ? 1 : (ratio < 0 ? 0 : ratio);
            return min + (max - min) * ratio;
        }

        public float Ratio(float value) {
            value = value < min ? min : (value > max ? max : value);
            return (value - min) / max; 
        }

        public float Ratio(int value) {
            return Ratio((float)value);
        }

    }

}
