using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityGLTF;

public class ArController : MonoBehaviour
{
    [SerializeField] private ARSessionOrigin arOrigin;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject placementIndicator;
    [SerializeField] private GLTFComponent gltfPrefab;
    [SerializeField] bool calculateColliders;
    [SerializeField] string[] urls;

    private Pose placementPose;
    List<ARRaycastHit> hits = new List<ARRaycastHit>(32);

    public bool PlacemetPoseValid { get; private set; }
    public static ArController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        UIController.Instance.SetupButtons(urls, this);
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

    }

    private void UpdatePlacementIndicator()
    {
        placementIndicator.SetActive(PlacemetPoseValid);
        if (PlacemetPoseValid)
        {
            placementIndicator.transform.position = placementPose.position;
            Vector3 planeNormal = placementPose.rotation * Vector3.up;
            Vector3 lookFwd = Vector3.ProjectOnPlane(arOrigin.camera.transform.forward, planeNormal).normalized;
            placementIndicator.transform.rotation = Quaternion.LookRotation(lookFwd, planeNormal);
        } 
    }

    private void UpdatePlacementPose()
    {
        hits.Clear();
        arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height/2), hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        PlacemetPoseValid = hits.Count > 0;
        if (PlacemetPoseValid)
        {
            placementPose = hits[0].pose;
        }
    }

    public void LoadAndSpawnGLTFModel(string url)
    {
        LoadAndSpawnGLTFModel(url, placementIndicator.transform.position, placementIndicator.transform.rotation);
    }

    void LoadAndSpawnGLTFModel(string url, Vector3 position, Quaternion rotation)
    {
        if (gltfPrefab)
        {
            GLTFComponent gltfSpawner = Instantiate(gltfPrefab);
            gltfSpawner.GLTFUri = url;
            gltfSpawner.transform.position = position;
            gltfSpawner.transform.rotation = rotation;
            gltfSpawner.onLoadFinished += OnGLTFModelLoaded;
        }
    }

    void OnGLTFModelLoaded(GLTFComponent model)
    {
        if (calculateColliders)
        {
            InstantiatedGLTFObject rootObject = model.GetComponentInChildren<InstantiatedGLTFObject>();

            Collider[] colliders = rootObject.GetComponentsInChildren<Collider>();

            if (colliders.Length > 0)
            {
                float lowestPointInBounds = float.PositiveInfinity;

                foreach (var c in colliders)
                {
                    float lowestPoint = c.transform.position.y - c.bounds.center.y - c.bounds.extents.y;
                    if (lowestPoint < lowestPointInBounds)
                        lowestPointInBounds = lowestPoint;
                }

                Vector3 p = rootObject.transform.position;
                p.y += p.y - lowestPointInBounds;

                rootObject.transform.position = p;
            }
        }
    }
}
