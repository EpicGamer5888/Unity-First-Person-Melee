using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AlertStage
{
    Peaceful,
    Intrigued,
    Alerted
}

public class EnemyManager : MonoBehaviour
{

    public float fov;
    [Range(0, 360)] public float fovAngle; // in degrees

    public AlertStage alertStage;
    [Range(0, 5)] public float alertLevel; // 0: Peaceful, 100: Alerted

    private void Awake()
    {
        alertStage = AlertStage.Peaceful;
        alertLevel = 5;

    }

    private void Update()
    {

        bool playerInFOV = false;
        Collider[] targetsInFOV = Physics.OverlapSphere(
            transform.position, fov);
        foreach (Collider c in targetsInFOV)
        {
            if (c.CompareTag("Player"))
            {
                float signedAngle = Vector3.Angle(
                    transform.forward,
                    c.transform.position - transform.position);
                if (Mathf.Abs(signedAngle) < fovAngle / 2)
                    playerInFOV = true;
                break;
            }
        }

        _UpdateAlertState(playerInFOV);
    }

    private void _UpdateAlertState(bool playerInFOV)
    {
        switch (alertStage)
        {
            case AlertStage.Peaceful:
                if (playerInFOV)
                    alertStage = AlertStage.Intrigued;
                break;
            case AlertStage.Intrigued:
                if (playerInFOV)
                {
<<<<<<< HEAD
                    alertLevel += 10f;
                    if (alertLevel >= 100)
=======
                    alertLevel++;
                    if (alertLevel >= 5)
>>>>>>> 00f540eec192ae5c741921e3e50b07cd69426778
                    {
                        alertStage = AlertStage.Alerted;
                    }

                }
                else
                {
                    alertLevel--;
                    if (alertLevel <= 0)
                        alertStage = AlertStage.Peaceful;
                }
                break;
            case AlertStage.Alerted:
                if (!playerInFOV)
                    alertStage = AlertStage.Intrigued;
                break;
        }
    }

}