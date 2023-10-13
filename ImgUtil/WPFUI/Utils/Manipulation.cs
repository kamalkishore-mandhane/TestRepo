using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ImgUtil.WPFUI.Utils
{
    /// <summary>
    /// Manipulation
    /// </summary>
    public enum MANIPULATION_PROCESSOR_MANIPULATIONS
    {
        MANIPULATION_NONE = 0,
        MANIPULATION_TRANSLATE_X = 0x1,
        MANIPULATION_TRANSLATE_Y = 0x2,
        MANIPULATION_SCALE = 0x4,
        MANIPULATION_ROTATE = 0x8,
        MANIPULATION_ALL = 0xf
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4f62c8da-9c53-4b22-93df-927a862bbb03")]
    internal interface IManipulationEvents
    {
        void ManipulationStarted(float x, float y);

        void ManipulationDelta(
            float x,
            float y,
            float translationDeltaX,
            float translationDeltaY,
            float scaleDelta,
            float expansionDelta,
            float rotationDelta,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation);

        void ManipulationCompleted(
            float x,
            float y,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation);
    }

    [ComVisible(false)]
    public delegate void ManipulationEvents_ManipulationStartedEventHandler(float x, float y);

    [ComVisible(false)]
    public delegate void ManipulationEvents_ManipulationDeltaEventHandler(float x, float y, float translationDeltaX, float translationDeltaY, float scaleDelta, float expansionDelta, float rotationDelta, float cumulativeTranslationX, float cumulativeTranslationY, float cumulativeScale, float cumulativeExpansion, float cumulativeRotation);

    [ComVisible(false)]
    public delegate void ManipulationEvents_ManipulationCompletedEventHandler(float x, float y, float cumulativeTranslationX, float cumulativeTranslationY, float cumulativeScale, float cumulativeExpansion, float cumulativeRotation);

    [ComVisible(false)]
    [ComEventInterface(typeof(IManipulationEvents), typeof(ManipulationEvents_EventProvider))]
    public interface IManipulationEvents_Event
    {
        event ManipulationEvents_ManipulationStartedEventHandler ManipulationStarted;
        event ManipulationEvents_ManipulationDeltaEventHandler ManipulationDelta;
        event ManipulationEvents_ManipulationCompletedEventHandler ManipulationCompleted;
    }

    [ComVisible(false)]
    [ClassInterface(ClassInterfaceType.None)]
    internal sealed class ManipulationEvents_EventProvider : IManipulationEvents_Event, IDisposable, IManipulationEvents
    {
        private IConnectionPoint _cp;
        private int _cookie;

        private ManipulationEvents_ManipulationStartedEventHandler _manipulationStarted;
        private ManipulationEvents_ManipulationDeltaEventHandler _manipulationDelta;
        private ManipulationEvents_ManipulationCompletedEventHandler _manipulationCompleted;

        public ManipulationEvents_EventProvider(IConnectionPointContainer cpc)
        {
            Guid guid = new Guid("4f62c8da-9c53-4b22-93df-927a862bbb03");
            cpc.FindConnectionPoint(ref guid, out _cp);
            _cp.Advise(this, out _cookie);
        }
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        ~ManipulationEvents_EventProvider()
        {
            Dispose(true);
        }

        private void Dispose(bool finialize)
        {
            if (_cp != null)
            {
                try
                {
                    _cp.Unadvise(_cookie);
                    Marshal.ReleaseComObject(_cp);
                    _cp = null;
                }
                catch
                {
                }
            }
        }

        void IManipulationEvents.ManipulationStarted(float x, float y)
        {
            if (_manipulationStarted != null)
            {
                _manipulationStarted(x, y);
            }
        }

        void IManipulationEvents.ManipulationDelta(
            float x,
            float y,
            float translationDeltaX,
            float translationDeltaY,
            float scaleDelta,
            float expansionDelta,
            float rotationDelta,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation)
        {
            if (_manipulationDelta != null)
            {
                _manipulationDelta(x, y, translationDeltaX, translationDeltaY, scaleDelta, expansionDelta, rotationDelta, cumulativeTranslationX, cumulativeTranslationY, cumulativeScale, cumulativeExpansion, cumulativeRotation);
            }
        }

        void IManipulationEvents.ManipulationCompleted(
            float x,
            float y,
            float cumulativeTranslationX,
            float cumulativeTranslationY,
            float cumulativeScale,
            float cumulativeExpansion,
            float cumulativeRotation)
        {
            if (_manipulationCompleted != null)
            {
                _manipulationCompleted(x, y, cumulativeTranslationX, cumulativeTranslationY, cumulativeScale, cumulativeExpansion, cumulativeRotation);
            }
        }

        event ManipulationEvents_ManipulationStartedEventHandler IManipulationEvents_Event.ManipulationStarted
        {
            add
            {
                _manipulationStarted += value;
            }
            remove
            {
                _manipulationStarted -= value;
            }
        }

        event ManipulationEvents_ManipulationDeltaEventHandler IManipulationEvents_Event.ManipulationDelta
        {
            add
            {
                _manipulationDelta += value;
            }
            remove
            {
                _manipulationDelta -= value;
            }
        }

        event ManipulationEvents_ManipulationCompletedEventHandler IManipulationEvents_Event.ManipulationCompleted
        {
            add
            {
                _manipulationCompleted += value;
            }
            remove
            {
                _manipulationCompleted -= value;
            }
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("A22AC519-8300-48a0-BEF4-F1BE8737DBA4")]
    public interface IManipulationProcessor_
    {
        MANIPULATION_PROCESSOR_MANIPULATIONS get_SupportedManipulations();

        void put_SupportedManipulations(MANIPULATION_PROCESSOR_MANIPULATIONS manipulations);

        float get_PivotPointX();

        void put_PivotPointX(float pivotPointX);

        float get_PivotPointY();

        void put_PivotPointY(float pivotPointY);

        float get_PivotRadius();

        void put_PivotRadius(float pivotRadius);

        void CompleteManipulation();

        void ProcessDown(int manipulatorId, float x, float y);

        void ProcessMove(int manipulatorId, float x, float y);

        void ProcessUp(int manipulatorId, float x, float y);

        void ProcessDownWithTime(int manipulatorId, float x, float y, int timestamp);

        void ProcessMoveWithTime(int manipulatorId, float x, float y, int timestamp);

        void ProcessUpWithTime(int manipulatorId, float x, float y, int timestamp);

        float GetVelocityX();

        float GetVelocityY();

        float GetExpansionVelocity();

        float GetAngularVelocity();

        float get_MinimumScaleRotateRadius();

        void put_MinimumScaleRotateRadius(float minRadius);
    }

    [ComImport]
    [CoClass(typeof(ManipulationProcessorClass))]
    [Guid("A22AC519-8300-48a0-BEF4-F1BE8737DBA4")]
    public interface IManipulationProcessor : IManipulationProcessor_, IManipulationEvents_Event
    {
    }

    [ComImport]
    [Guid("597D4FB0-47FD-4aff-89B9-C6CFAE8CF08E")]
    internal class ManipulationProcessorClass
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("18b00c6d-c5ee-41b1-90a9-9d4a929095ad")]
    public interface IInertiaProcessor_
    {
        float get_InitialOriginX();

        void put_InitialOriginX(float x);

        float get_InitialOriginY();

        void put_InitialOriginY(float y);

        float get_InitialVelocityX();

        void put_InitialVelocityX(float x);

        float get_InitialVelocityY();

        void put_InitialVelocityY(float y);

        float get_InitialAngularVelocity();

        void put_InitialAngularVelocity(float velocity);

        float get_InitialExpansionVelocity();

        void put_InitialExpansionVelocity(float velocity);

        float get_InitialRadius();

        void put_InitialRadius(float radius);

        float get_BoundaryLeft();

        void put_BoundaryLeft(float left);

        float get_BoundaryTop();

        void put_BoundaryTop(float top);

        float get_BoundaryRight();

        void put_BoundaryRight(float right);

        float get_BoundaryBottom();

        void put_BoundaryBottom(float bottom);

        float get_ElasticMarginLeft();

        void put_ElasticMarginLeft(float left);

        float get_ElasticMarginTop();

        void put_ElasticMarginTop(float top);

        float get_ElasticMarginRight();

        void put_ElasticMarginRight(float right);

        float get_ElasticMarginBottom();

        void put_ElasticMarginBottom(float bottom);

        float get_DesiredDisplacement();

        void put_DesiredDisplacement(float displacement);

        float get_DesiredRotation();

        void put_DesiredRotation(float rotation);

        float get_DesiredExpansion();

        void put_DesiredExpansion(float expansion);

        float get_DesiredDeceleration();

        void put_DesiredDeceleration(float deceleration);

        float get_DesiredAngularDeceleration();

        void put_DesiredAngularDeceleration(float deceleration);

        float get_DesiredExpansionDeceleration();

        void put_DesiredExpansionDeceleration(float deceleration);

        int get_InitialTimestamp();

        void put_InitialTimestamp(int timestamp);

        void Reset();

        bool Process();

        bool ProcessTime(int timestamp);

        void Complete();

        void CompleteTime(int timestamp);
    }

    [ComImport]
    [CoClass(typeof(InertiaProcessorClass))]
    [Guid("18b00c6d-c5ee-41b1-90a9-9d4a929095ad")]
    public interface IInertiaProcessor : IInertiaProcessor_, IManipulationEvents_Event
    {
    }

    [ComImport]
    [Guid("abb27087-4ce0-4e58-a0cb-e24df96814be")]
    internal class InertiaProcessorClass
    {
    }
}
