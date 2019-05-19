using UnityEngine;

namespace Script
{

    public class TurnIndicator : MonoBehaviour
    {
        private float currentLerpTime;

        private readonly float lerpTime = 0.75f;
        private Vector3 tiCurPos;
        private Vector3 tiNewPos;

        // Use this for initialization
        private void Start()
        {
            tiCurPos = tiNewPos = transform.position;
        }

        // Update is called once per frame
        private void Update()
        {
            //increment timer once per frame
            if (currentLerpTime > lerpTime)
            {
                currentLerpTime = 0;
                tiCurPos = tiNewPos = transform.position;
            }

            if (tiCurPos != tiNewPos)
            {
                currentLerpTime += Time.deltaTime;
                //lerp!
                var perc = currentLerpTime / lerpTime;
                transform.position = Vector3.Lerp(tiCurPos, tiNewPos, perc);
            }
        }

        public void setColour(string teamColour)
        {
            switch (teamColour)
            {
                case "red":
                    GetComponent<Renderer>().material.color = Color.red;
                    //TODO - Remove hardcoding of indicator position
                    tiNewPos.x = -2.2f;
                    break;

                case "blue":
                    GetComponent<Renderer>().material.color = Color.blue;
                    //TODO - Remove hardcoding of indicator position
                    tiNewPos.x = 2.2f;
                    break;

                default:
                    Debug.Log("Error - unknown team colour");
                    break;
            }

            transform.position = Vector3.Lerp(tiCurPos, tiNewPos, Time.time / 1.0f);
        }
    }
}