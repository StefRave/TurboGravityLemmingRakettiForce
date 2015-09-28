using System;
using System.Collections;
using System.Collections.Generic;

namespace DxVBLibA
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CoClass(typeof(DirectX8Class)), Guid("E7FF1301-96A5-11D3-AC85-00C04FC2C602")]
    public interface DirectX8 : IDirectX8
    {
    }

    [ComImport, Guid("E7FF1300-96A5-11D3-AC85-00C04FC2C602"),
        ClassInterface((short)0),
        TypeLibType((short)2)]
    public class DirectX8Class : IDirectX8, DirectX8
    {

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern DirectInput8 DirectInputCreate();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void DirectSoundCaptureCreate([In, MarshalAs(UnmanagedType.BStr)] string guid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void DirectSoundCreate([In, MarshalAs(UnmanagedType.BStr)] string guid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetDSCaptureEnum();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetDSEnum();
    }

    [ComImport, Guid("E7FF1301-96A5-11D3-AC85-00C04FC2C602"), InterfaceType((short)1)]
    public interface IDirectX8
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DirectSoundCreate([In, MarshalAs(UnmanagedType.BStr)] string guid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DirectSoundCaptureCreate([In, MarshalAs(UnmanagedType.BStr)] string guid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDSEnum();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDSCaptureEnum();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        DirectInput8 DirectInputCreate();
    }

    [ComImport, Guid("819D20C1-8AD2-11D3-AC85-00C04FC2C602"), InterfaceType((short)1)]
    public interface DirectInput8
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40)]
        void InternalSetObject([In, MarshalAs(UnmanagedType.IUnknown)] object lpdd);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc((short)0x40)]
        object InternalGetObject();

        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        DirectInputDevice8 CreateDevice([In, MarshalAs(UnmanagedType.BStr)] string guid);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        DirectInputEnumDevices8 GetDIDevices([In] CONST_DI8DEVICETYPE DeviceType, [In] CONST_DIENUMDEVICESFLAGS flags);
    }


    [ComImport, Guid("819D20C4-8AD2-11D3-AC85-00C04FC2C602"), InterfaceType((short) 1)]
    public interface DirectInputEnumDevices8
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        DirectInputDeviceInstance8 GetItem([In] int index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetCount();
    }

    [ComImport, Guid("819D20C2-8AD2-11D3-AC85-00C04FC2C602"), InterfaceType((short) 1)]
    public interface DirectInputDeviceInstance8
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string GetGuidInstance();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string GetGuidProduct();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string GetProductName();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string GetInstanceName();
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        string GetGuidFFDriver();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        short GetUsagePage();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        short GetUsage();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetDevType();
    }

    [ComImport, InterfaceType((short) 1), Guid("819D20C3-8AD2-11D3-AC85-00C04FC2C602")]
    public interface DirectInputDevice8
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40)]
        void InternalSetObject([In, MarshalAs(UnmanagedType.IUnknown)] object lpdd);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime), TypeLibFunc((short) 0x40)]
        object InternalGetObject();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Acquire();
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        /*DirectInputEnumDeviceObjects */ void GetDeviceObjectsEnum([In] /*CONST_DIDFTFLAGS*/ int flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCapabilities([In, Out] ref DIDEVCAPS Caps);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        int GetDeviceData([In, Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_RECORD)] ref Array deviceObjectDataArray, [In] /*CONST_DIDGDDFLAGS*/ IntPtr flags);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        DirectInputDeviceInstance8 GetDeviceInfo();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceStateKeyboard([In, Out] ref /*DIKEYBOARDSTATE */ IntPtr state);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceStateMouse([In, Out] ref /*DIMOUSESTATE*/ IntPtr state);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceStateMouse2([In, Out] ref /*DIMOUSESTATE2*/ IntPtr state);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceStateJoystick([In, Out] ref DIJOYSTATE state);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceStateJoystick2([In, Out] ref DIJOYSTATE2 state);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDeviceState([In] int cb, [In] IntPtr state);
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        /*DirectInputDeviceObjectInstance*/ IntPtr GetObjectInfo([In] int obj, [In] /*CONST_DIPHFLAGS*/ IntPtr how);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetProperty([In, MarshalAs(UnmanagedType.BStr)] string guid, [Out] IntPtr propertyInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RunControlPanel([In] int hwnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCooperativeLevel([In] int hwnd, [In] CONST_DISCLFLAGS flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCommonDataFormat([In] CONST_DICOMMONDATAFORMATS Format);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetDataFormat([In] ref /*DIDATAFORMAT*/ IntPtr Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_RECORD)] ref Array formatArray);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEventNotification([In] int hEvent);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetProperty([In, MarshalAs(UnmanagedType.BStr)] string guid, [In] IntPtr propertyInfo);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Unacquire();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Poll();
    }

    public enum CONST_DI8DEVICETYPE
    {
        DI8DEVCLASS_ALL = 0,
        DI8DEVCLASS_DEVICE = 1,
        DI8DEVCLASS_GAMECTRL = 4,
        DI8DEVCLASS_KEYBOARD = 3,
        DI8DEVCLASS_POINTER = 2,
        DI8DEVTYPE_1STPERSON = 0x18,
        DI8DEVTYPE_DEVICE = 0x11,
        DI8DEVTYPE_DEVICECTRL = 0x19,
        DI8DEVTYPE_DRIVING = 0x16,
        DI8DEVTYPE_FLIGHT = 0x17,
        [TypeLibVar((short)0x40)]
        DI8DEVTYPE_FORCEDWORD = 0x7f000000,
        DI8DEVTYPE_GAMEPAD = 0x15,
        DI8DEVTYPE_JOYSTICK = 20,
        DI8DEVTYPE_KEYBOARD = 0x13,
        DI8DEVTYPE_MOUSE = 0x12,
        DI8DEVTYPE_REMOTE = 0x1b,
        DI8DEVTYPE_SCREENPOINTER = 0x1a,
        DI8DEVTYPE_SUPPLEMENTAL = 0x1c
    }
    
    public enum CONST_DICOMMONDATAFORMATS
    {
        DIFORMAT_JOYSTICK = 3,
        DIFORMAT_JOYSTICK2 = 4,
        DIFORMAT_KEYBOARD = 1,
        DIFORMAT_MOUSE = 2,
        DIFORMAT_MOUSE2 = 5
    }
    public enum CONST_DIDEVCAPSFLAGS
    {
        DIDC_ALIAS = 0x10000,
        DIDC_ATTACHED = 1,
        DIDC_DEADBAND = 0x4000,
        DIDC_EMULATED = 4,
        DIDC_FFATTACK = 0x200,
        DIDC_FFFADE = 0x400,
        DIDC_FORCEFEEDBACK = 0x100,
        DIDC_HIDDEN = 0x40000,
        DIDC_PHANTOM = 0x20000,
        DIDC_POLLEDDATAFORMAT = 8,
        DIDC_POLLEDDEVICE = 2,
        DIDC_POSNEGCOEFFICIENTS = 0x1000,
        DIDC_POSNEGSATURATION = 0x2000,
        DIDC_SATURATION = 0x800,
        DIDC_STARTDELAY = 0x8000
    }
    public enum CONST_DIENUMDEVICESFLAGS
    {
        DIEDFL_ALLDEVICES = 0,
        DIEDFL_ATTACHEDONLY = 1,
        DIEDFL_FORCEFEEDBACK = 0x100,
        DIEDFL_INCLUDEALIASES = 0x10000,
        DIEDFL_INCLUDEHIDDEN = 0x40000,
        DIEDFL_INCLUDEPHANTOMS = 0x20000
    }
    public enum CONST_DISCLFLAGS
    {
        DISCL_BACKGROUND = 8,
        DISCL_EXCLUSIVE = 1,
        DISCL_FOREGROUND = 4,
        DISCL_NONEXCLUSIVE = 2,
        DISCL_NOWINKEY = 0x10
    }

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct DIDEVCAPS
    {
        [TypeLibVar((short) 0x40)]
        public int lSize;
        public CONST_DIDEVCAPSFLAGS lFlags;
        public int lDevType;
        public int lAxes;
        public int lButtons;
        public int lPOVs;
        public int lFFSamplePeriod;
        public int lFFMinTimeResolution;
        public int lFirmwareRevision;
        public int lHardwareRevision;
        public int lDriverVersion;
    }
    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct DIJOYSTATE
    {
        public int x;
        public int y;
        public int z;
        public int rx;
        public int ry;
        public int rz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[] slider;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public int[] POV;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
        public byte[] Buttons;
    }
    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct DIJOYSTATE2
    {
        public int x;
        public int y;
        public int z;
        public int rx;
        public int ry;
        public int rz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[] slider;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public int[] POV;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x80)]
        public byte[] Buttons;
        public int vx;
        public int vy;
        public int vz;
        public int vrx;
        public int vry;
        public int vrz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[] vslider;
        public int ax;
        public int ay;
        public int az;
        public int arx;
        public int ary;
        public int arz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[] aslider;
        public int fx;
        public int fy;
        public int fz;
        public int frx;
        public int fry;
        public int frz;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[] fslider;
    }

}

