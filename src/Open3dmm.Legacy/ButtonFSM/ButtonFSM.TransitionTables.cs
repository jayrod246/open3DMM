using static Open3dmm.ButtonFSM.States;

namespace Open3dmm
{
    partial class ButtonFSM
    {
        private static readonly Transition[] InverseTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ 0,
        /* UpOff     */ Ready,
        /* UpOffOn   */ UpOff,
        /* UpOnOff   */ UpOn,
        /* UpOn      */ UpOffOn,
        /* DownUpOff */ UpOff,
        /* UpDownOn  */ UpOn,
        /* DownUpOn  */ DownOn,
        /* DownOn    */ UpDownOn,
        /* DownOnOff */ DownOn,
        /* DownOffOn */ DownOff,
        /* DownOff   */ DownOnOff,
        /* Click     */ DownUpOn,
    };

        private static readonly Transition[] DefaultTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ Ready,
        /* Ready     */ UpOff,
        /* UpOff     */ UpOff,
        /* UpOffOn   */ UpOn,
        /* UpOnOff   */ UpOff,
        /* UpOn      */ UpOn,
        /* DownUpOff */ UpOff,
        /* UpDownOn  */ DownOn,
        /* DownUpOn  */ (Click, raiseClick: true),
        /* DownOn    */ DownOn,
        /* DownOnOff */ DownOff,
        /* DownOffOn */ DownOn,
        /* DownOff   */ DownOff,
        /* Click     */ Click,
    };

        private static readonly Transition[] MousePressedTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ UpOff,
        /* UpOff     */ UpOffOn,
        /* UpOffOn   */ UpOn,
        /* UpOnOff   */ UpOff,
        /* UpOn      */ UpDownOn,
        /* DownUpOff */ UpOff,
        /* UpDownOn  */ 0,
        /* DownUpOn  */ 0,
        /* DownOn    */ 0,
        /* DownOnOff */ 0,
        /* DownOffOn */ 0,
        /* DownOff   */ 0,
        /* Click     */ 0,
    };

        private static readonly Transition[] MouseUpOnTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ 0,
        /* UpOff     */ 0,
        /* UpOffOn   */ 0,
        /* UpOnOff   */ 0,
        /* UpOn      */ 0,
        /* DownUpOff */ 0,
        /* UpDownOn  */ DownOn,
        /* DownUpOn  */ DownUpOn,
        /* DownOn    */ DownUpOn,
        /* DownOnOff */ DownOff,
        /* DownOffOn */ DownOn,
        /* DownOff   */ DownUpOff,
        /* Click     */ UpOn,
    };

        private static readonly Transition[] MouseUpOffTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ 0,
        /* UpOff     */ 0,
        /* UpOffOn   */ 0,
        /* UpOnOff   */ 0,
        /* UpOn      */ 0,
        /* DownUpOff */ 0,
        /* UpDownOn  */ DownOn,
        /* DownUpOn  */ DownUpOn,
        /* DownOn    */ DownOnOff,
        /* DownOnOff */ DownOff,
        /* DownOffOn */ DownOn,
        /* DownOff   */ DownUpOff,
        /* Click     */ UpOn,
    };

        private static readonly Transition[] MouseDownOnTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ 0,
        /* UpOff     */ 0,
        /* UpOffOn   */ 0,
        /* UpOnOff   */ 0,
        /* UpOn      */ 0,
        /* DownUpOff */ 0,
        /* UpDownOn  */ UpDownOn,
        /* DownUpOn  */ DownUpOn,
        /* DownOn    */ DownOn,
        /* DownOnOff */ DownOff,
        /* DownOffOn */ DownOffOn,
        /* DownOff   */ DownOffOn,
        /* Click     */ UpOn,
    };

        private static readonly Transition[] MouseDownOffTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ 0,
        /* UpOff     */ 0,
        /* UpOffOn   */ 0,
        /* UpOnOff   */ 0,
        /* UpOn      */ 0,
        /* DownUpOff */ 0,
        /* UpDownOn  */ DownOn,
        /* DownUpOn  */ DownUpOn,
        /* DownOn    */ DownOnOff,
        /* DownOnOff */ DownOnOff,
        /* DownOffOn */ DownOn,
        /* DownOff   */ DownOff,
        /* Click     */ UpOn,
    };

        private static readonly Transition[] MouseOffTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ Ready,
        /* UpOff     */ UpOff,
        /* UpOffOn   */ UpOn,
        /* UpOnOff   */ UpOnOff,
        /* UpOn      */ UpOnOff,
        /* DownUpOff */ DownUpOff,
        /* UpDownOn  */ 0,
        /* DownUpOn  */ 0,
        /* DownOn    */ 0,
        /* DownOnOff */ 0,
        /* DownOffOn */ 0,
        /* DownOff   */ 0,
        /* Click     */ UpOn,
    };

        private static readonly Transition[] MouseOnTable = {
        /* BEFORE:      AFTER: */
        /* 0         */ 0,
        /* Ready     */ UpOff,
        /* UpOff     */ UpOffOn,
        /* UpOffOn   */ UpOffOn,
        /* UpOnOff   */ UpOff,
        /* UpOn      */ UpOn,
        /* DownUpOff */ UpOff,
        /* UpDownOn  */ 0,
        /* DownUpOn  */ 0,
        /* DownOn    */ 0,
        /* DownOnOff */ 0,
        /* DownOffOn */ 0,
        /* DownOff   */ 0,
        /* Click     */ 0,
    };
    }
}
