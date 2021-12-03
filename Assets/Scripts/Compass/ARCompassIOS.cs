using UnityEngine;

namespace UnityARCompass
{
    public class ARCompassIOS : MonoBehaviour, ICompass
    {
        private Camera _mainCamera;
        private double _lastCompassTimestamp;
        [SerializeField] public DirectionGenerator DirectionGenerator;
        private float tempDirection = 0.0f;

        public Quaternion TrueHeadingRotation { get; private set; } = Quaternion.identity;
        [HideInInspector]
        public float startLat;
        [HideInInspector]
        public float startLng;
        [HideInInspector]
        public float endLat;
        [HideInInspector]
        public float endLng;
        
        #region Unity Callback

        private void Start()
        {
            Input.compass.enabled = true;
            Input.location.Start();
            _mainCamera = Camera.main;
            Debug.Log("check _mainCamera in start()"+_mainCamera.transform.rotation);
        }

        private void Update()
        {
            if (!(Input.compass.timestamp > _lastCompassTimestamp)) return;
            _lastCompassTimestamp = Input.compass.timestamp;

            var direction = -(Input.compass.trueHeading - Input.compass.magneticHeading)
                                - DirectionGenerator.angleFromCoordinate(startLat, startLng, endLat, endLng);

            //Debug.Log("list all"+startLat+" "+startLng+" "+endLat+" "+endLng);
            //Debug.Log(direction);
            // avoid shaking
            if(Mathf.Abs(tempDirection - direction) < 2.0f)
                direction = tempDirection;
            
            // iOSだとx軸右、y軸上、z軸画面手前
            UpdateRotation(
                new Vector3(Input.compass.rawVector.x, Input.compass.rawVector.y, -Input.compass.rawVector.z),
                direction);

            tempDirection = direction;
        }

        #endregion

        private void UpdateRotation(Vector3 rawVector, float direction)
        {
            //Debug.Log("UpdateRotation called");
            // compensate camera pose
            //Debug.Log("direction"+direction);
            //Debug.Log("check _mainCamera"+_mainCamera.transform.rotation);
            //Debug.Log("check rawVector"+rawVector);

            rawVector = _mainCamera.transform.rotation * rawVector;
            // projection onto xz plane
            var xzProjection =
                new Vector3(rawVector.x, 0, rawVector.z);

            var trueHeading = Quaternion.Euler(0, direction, 0) * xzProjection.normalized;
            //Debug.Log("-------------trueHeading"+trueHeading);
            // update global rotation
            TrueHeadingRotation =
                Quaternion.FromToRotation(Vector3.forward, trueHeading);
            //Debug.Log("--------------TrueHeadingRotation"+TrueHeadingRotation);
        }
    }
}