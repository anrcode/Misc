using System;

using Microsoft.WindowsAPICodePack.Sensors;


namespace Gbi.Input
{
    /// <summary>
    /// This class captures input coming from a Flexis JM Badge Board.
    /// </summary>
    /// <remarks>More info at <![CDATA[http://www.freescale.com/webapp/sps/site/prod_summary.jsp?code=JMBADGE&fsrch=1&sr=3]]></remarks>
    public sealed class FlexisInputProvider : IInputProvider, IDisposable
    {
        private UnknownSensor       leftSwitchArraySensor   = null;
        private UnknownSensor       rightSwitchArraySensor  = null;
        private AmbientLightSensor  ambientLightSensor      = null;
        private Accelerometer3D     accelerometerSensor     = null;

        private Single pitchThrottleValue;
        private Single rollThrottleValue;
        private Single heightThrottleValue;
        private Single yawThrottleValue;

        private const string leftSwitchArraySensorId = "72b5fc25-f933-4ac7-b22b-350a39366ada";
        private const string rightSwitchArraySensorId = "1f9de627-c687-4146-a2d0-4a0a9ff90226";
        private const string ambientLightSensorId = "1a52edd0-297d-4325-9510-4eb3d9e43f13";
        private const string accelerometerSensorId = "5d376294-c5fc-4bba-859c-b26ca516fbfb";
        private Guid switchArrayValueKey = new Guid("38564a7c-f2f2-49bb-9b2b-ba60f66a58df");

        /// <summary>
        /// Initializes a new instance of the <see cref="FlexisInputProvider"/> class.
        /// </summary>
        /// <param name="droneCommander">The drone commander.</param>
        public FlexisInputProvider()
        {

        }

        /// <summary>
        /// Initializes the specified input provider.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the provider was initialized correclty; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>The throttling values are implicitly set to 1, so the values registered by the inputprovider will be passed unchanged to the object that implements the <see cref="IDroneCommander">IDroneCommander</see> interface.</remarks>
        public bool Initialize()
        {
            return Initialize(1, 1, 1, 1);
        }

        /// <summary>
        /// Initializes the specified input provider.
        /// </summary>
        /// <param name="rollThrottle">The roll throttle value between 0 and 1. The actual throttle value registered by the inputprovider will be multiplied by this value.</param>
        /// <param name="pitchThrottle">The pitch throttle value between 0 and 1. The actual pitch value registered by the inputprovider will be multiplied by this value.</param>
        /// <param name="heightThrottle">The height throttle value between 0 and 1. The actual height value registered by the inputprovider will be multiplied by this value.</param>
        /// <param name="yawThrottle">The yaw throttle value between 0 and 1. The actual yaw value registered by the inputprovider will be multiplied by this value.</param>
        /// <returns>
        /// Returns <c>true</c> if the provider was initialized correclty; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>The throttling is applied in order to be able to control the ARDrone in smaller places. A throttle value of 1 means that there is no throttling, a value of 0 means that the resulting value will also be 0.</remarks>
        public bool Initialize(float rollThrottle, float pitchThrottle, float heightThrottle, float yawThrottle)
        {
            this.rollThrottleValue = rollThrottle;
            this.pitchThrottleValue = pitchThrottle;
            this.heightThrottleValue = heightThrottle;
            this.yawThrottleValue = yawThrottle;

            leftSwitchArraySensor   = SensorManager.GetSensorBySensorId<UnknownSensor>(new Guid(leftSwitchArraySensorId));
            rightSwitchArraySensor  = SensorManager.GetSensorBySensorId<UnknownSensor>(new Guid(rightSwitchArraySensorId));
            ambientLightSensor      = SensorManager.GetSensorBySensorId<AmbientLightSensor>(new Guid(ambientLightSensorId));
            accelerometerSensor     = SensorManager.GetSensorBySensorId<Accelerometer3D>(new Guid(accelerometerSensorId));

            leftSwitchArraySensor.DataReportChanged += new DataReportChangedEventHandler(SensorDataChanged);
            rightSwitchArraySensor.DataReportChanged += new DataReportChangedEventHandler(SensorDataChanged);
            ambientLightSensor.DataReportChanged += new DataReportChangedEventHandler(SensorDataChanged);
            accelerometerSensor.DataReportChanged += new DataReportChangedEventHandler(SensorDataChanged);

            return true;
        }

        /// <summary>
        /// Triggered when the sensor detects a change in data.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SensorDataChanged(Sensor sender, EventArgs e)
        {
            float pitch = 0;
            float roll = 0;
            float yaw = 0;
            float height = 0;

            if (leftSwitchArraySensor == null || rightSwitchArraySensor == null || accelerometerSensor == null || accelerometerSensor == null)
            {
                return;
            }

            if (leftSwitchArraySensor.DataReport == null)
            {
                leftSwitchArraySensor.UpdateData();
            }

            if (rightSwitchArraySensor.DataReport == null)
            {
                rightSwitchArraySensor.UpdateData();
            }

            if (ambientLightSensor.DataReport == null)
            {
                ambientLightSensor.UpdateData();
            }

            if (accelerometerSensor.DataReport == null)
            {
                accelerometerSensor.UpdateData();
            }

            object value = 0;
            int sensorValue = 0;

            value = leftSwitchArraySensor.DataReport.Values[switchArrayValueKey][0];
            sensorValue = Int32.Parse(value.ToString());
            switch (sensorValue)
            {
                case 0:
                    height = 0;
                    break;
                case 1:
                    height = -heightThrottleValue;
                    break;
                case 2:
                    yaw = -yawThrottleValue;
                    break;
                case 4:
                    break;
                case 8:
                    //DroneCommander.StartEngines();
                    break;
            }

            if (sensorValue == 0)
            {
                value = rightSwitchArraySensor.DataReport.Values[switchArrayValueKey][0];
                sensorValue = Int32.Parse(value.ToString());
                switch (sensorValue)
                {
                    case 0:
                        height = 0;
                        break;
                    case 1:
                        height = heightThrottleValue;
                        break;
                    case 2:
                        yaw = yawThrottleValue;
                        break;
                    case 4:
                        break;
                    case 8:
                        //DroneCommander.StopEngines();
                        break;
                }
            }

            roll = accelerometerSensor.CurrentAcceleration[AccelerationAxis.XAxis] * rollThrottleValue;
            pitch = - accelerometerSensor.CurrentAcceleration[AccelerationAxis.YAxis] * pitchThrottleValue;

            if (ambientLightSensor.CurrentLuminousIntensity.Intensity < 20)
            {
                height = .2f;                
            }

            //DroneCommander.SetFlightParameters(roll, pitch, height, yaw);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            leftSwitchArraySensor = null;   
            rightSwitchArraySensor = null; 
            ambientLightSensor = null;
            accelerometerSensor = null;     
        }
    }
}

