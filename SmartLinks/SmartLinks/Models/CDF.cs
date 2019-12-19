using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SmartLinks.Models
{
    public class CDF
    {
        public FileInfo cdf_File = null;
        public bool fileReadError = false;
        public string errorMsg = "";
        private static DateTime UnixTimeStampToDateTime(Int32 unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public Hdc_Struct Hdc;
        public Doc_Struct[] Doc;
        public Bth_Struct[] Bth;
        public Crr_Struct[] Crr;
        public Bdc_Struct[] Bdc;
        public Tdc_Struct[] Tdc;
        public Ddc_Struct[] Ddc;

        public CDF(FileInfo cdfFile, bool ignore_C106_Version = false)
        {
            this.cdf_File = cdfFile;
            getData(ignore_C106_Version);
        }

        public static void Write_CDF_File(CDF cdf, FileInfo fileInfo)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write)))
            {
                //Hdc records
                writer.Write(getBytes_FromString(cdf.Hdc._cdfver, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._operator, 12));
                DateTime t = DateTime.Now;
                writer.Write(UnixTimeStampFromDateTime(t));
                writer.Write(UnixTimeStampFromDateTime(t));
                writer.Write(getBytes_FromString(cdf.Hdc._mode, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._tstrtyp, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._tstrnum, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._statnum, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._flat, 4));
                writer.Write((Int16)cdf.Hdc._xcen);
                writer.Write((Int16)cdf.Hdc._ycen);
                writer.Write(getBytes_FromString(cdf.Hdc._swtyp, 14));
                writer.Write(getBytes_FromString(cdf.Hdc._swname, 14));
                writer.Write(getBytes_FromString(cdf.Hdc._swver, 6));
                writer.Write(cdf.Hdc._swdate);
                writer.Write(getBytes_FromString(cdf.Hdc._sitecode, 12));
                writer.Write(getBytes_FromString(cdf.Hdc._partfmly, 12));
                writer.Write(getBytes_FromString(cdf.Hdc._partdesig, 12));
                writer.Write(cdf.Hdc._xdim);
                writer.Write(cdf.Hdc._ydim);
                writer.Write(cdf.Hdc._bths);
                writer.Write(cdf.Hdc._crrs);
                writer.Write(cdf.Hdc._bdcs);
                writer.Write(cdf.Hdc._tdcs);
                writer.Write(cdf.Hdc._ddcs);
                writer.Write(cdf.Hdc._trcs);
                writer.Write(cdf.Hdc._xlocmin);
                writer.Write(cdf.Hdc._xlocmax);
                writer.Write(cdf.Hdc._ylocmin);
                writer.Write(cdf.Hdc._ylocmax);
                writer.Write(getBytes_FromString(cdf.Hdc._usvn, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._csvn, 6));
                writer.Write(cdf.Hdc._hdcs);
                writer.Write(cdf.Hdc._docs);
                writer.Write(cdf.Hdc._datalevel);
                writer.Write(getBytes_FromString(cdf.Hdc._mainrver, 6));
                writer.Write(UnixTimeStampFromDateTime(cdf.Hdc._mainrdate));
                writer.Write(getBytes_FromString(cdf.Hdc._reserved, 2));
                writer.Write(getBytes_FromString(cdf.Hdc._catlist, 40));
                writer.Write(getBytes_FromString(cdf.Hdc._customer, 40));
                writer.Write(getBytes_FromString(cdf.Hdc._workorder, 40));
                writer.Write(getBytes_FromString(cdf.Hdc._datecode, 6));
                writer.Write(getBytes_FromString(cdf.Hdc._Lot, 16));
                writer.Write(getBytes_FromString(cdf.Hdc._Sublot, 12));
                writer.Write(getBytes_FromString(cdf.Hdc._tstrdscr, 14));
                writer.Write((Int32)cdf.Hdc._devcnt2);
                writer.Write(getBytes_FromString(cdf.Hdc._future, 74));
                writer.Write(getBytes_FromString(cdf.Hdc._chksum, 8));

                //Doc Struct
                if (cdf.Hdc._docs > 0)
                {
                    for (int i = 0; i < cdf.Hdc._docs; i++)
                    {
                        writer.Write(getBytes_FromString(cdf.Doc[i]._txt, 82));
                    }

                }

                //Bth Struct
                if (cdf.Hdc._bths > 0)
                {
                    for (int i = 0; i < cdf.Hdc._bths; i++)
                    {
                        writer.Write(getBytes_FromString(cdf.Bth[i]._lot, 16));
                        writer.Write(getBytes_FromString(cdf.Bth[i]._sublot, 12));
                        writer.Write(getBytes_FromString(cdf.Bth[i]._future, 16));
                    }
                }

                //Crr Struct
                if (cdf.Hdc._crrs > 0)
                {
                    for (int i = 0; i < cdf.Hdc._crrs; i++)
                    {
                        writer.Write(getBytes_FromString(cdf.Crr[i]._partdesig, 12));
                        writer.Write(getBytes_FromString(cdf.Crr[i]._sernum, 12));
                        writer.Write(UnixTimeStampFromDateTime(cdf.Crr[i]._testdate));
                        writer.Write(getBytes_FromString(cdf.Crr[i]._testname, 12));
                        writer.Write(cdf.Crr[i]._idealval);
                        writer.Write(cdf.Crr[i]._actualval);
                        writer.Write(cdf.Crr[i]._deltamax);
                        writer.Write(cdf.Crr[i]._deltamin);
                        writer.Write(getBytes_FromString(cdf.Crr[i]._future, 16));
                    }
                }

                //Bdc Struct
                if (cdf.Hdc._bdcs > 0)
                {
                    for (int i = 0; i < cdf.Hdc._bdcs; i++)
                    {
                        writer.Write(getBytes_FromString(cdf.Bdc[i]._binname, 24));
                        writer.Write(cdf.Bdc[i]._binnum);
                        writer.Write(cdf.Bdc[i]._bincnt);
                        writer.Write(getBytes_FromString(cdf.Bdc[i]._future, 8));
                    }
                }

                //Tdc Struct
                if (cdf.Hdc._tdcs > 0)
                {
                    for (int i = 0; i < cdf.Hdc._tdcs; i++)
                    {
                        writer.Write(getBytes_FromString(cdf.Tdc[i]._testname, 24));
                        writer.Write(getBytes_FromString(cdf.Tdc[i]._statname, 12));
                        writer.Write(cdf.Tdc[i]._testnum);
                        writer.Write(getBytes_FromString(cdf.Tdc[i]._units, 6));
                        writer.Write(cdf.Tdc[i]._rangelo);
                        writer.Write(cdf.Tdc[i]._rangehi);
                        writer.Write(cdf.Tdc[i]._resolution);
                        writer.Write(cdf.Tdc[i]._lolimit);
                        writer.Write(cdf.Tdc[i]._hilimit);
                        writer.Write(cdf.Tdc[i]._anomfltrlo);
                        writer.Write(cdf.Tdc[i]._anomfltrhi);
                        writer.Write(cdf.Tdc[i]._execcnt);
                        writer.Write(cdf.Tdc[i]._failcnt);
                        writer.Write(cdf.Tdc[i]._minval);
                        writer.Write(cdf.Tdc[i]._maxval);
                        writer.Write(cdf.Tdc[i]._mean);
                        writer.Write(cdf.Tdc[i]._sigma);
                        writer.Write(cdf.Tdc[i]._skew);
                        writer.Write(cdf.Tdc[i]._kurt);
                        writer.Write(cdf.Tdc[i]._anomlo);
                        writer.Write(cdf.Tdc[i]._anomhi);
                        writer.Write(cdf.Tdc[i]._cvrgcnt);
                        writer.Write(cdf.Tdc[i]._statflg);
                        writer.Write(cdf.Tdc[i]._failcnt2);
                        writer.Write(getBytes_FromString(cdf.Tdc[i]._datatype, 2));
                        writer.Write(cdf.Tdc[i]._datasize);
                        writer.Write(cdf.Tdc[i]._statcnt);
                        writer.Write(cdf.Tdc[i]._median);
                        writer.Write(cdf.Tdc[i]._uhng);
                        writer.Write(cdf.Tdc[i]._lhng);
                        writer.Write(cdf.Tdc[i]._uwsk);
                        writer.Write(cdf.Tdc[i]._lwsk);
                        writer.Write(cdf.Tdc[i]._uifnc);
                        writer.Write(cdf.Tdc[i]._lifnc);
                        writer.Write(cdf.Tdc[i]._uofnc);
                        writer.Write(cdf.Tdc[i]._lofnc);
                        writer.Write(getBytes_FromString(cdf.Tdc[i]._future, 20));
                    }
                }

                //Ddc Struct
                if (cdf.Hdc._ddcs > 0)
                {
                    int count = cdf.Hdc._ddcs;
                    if (cdf.Hdc._devcnt2 > 0)
                        count = cdf.Hdc._devcnt2;
                    for (int i = 0; i < count; i++)
                    {
                        writer.Write(cdf.Ddc[i]._dienum);
                        writer.Write(cdf.Ddc[i]._xpos);
                        writer.Write(cdf.Ddc[i]._ypos);
                        writer.Write(cdf.Ddc[i]._bincode);
                        if (cdf.Hdc._trcs > 0)
                        {
                            for (int j = 0; j < cdf.Hdc._trcs; j++)
                            {
                                writer.Write(cdf.Ddc[i].Trc[j]._result);
                            }
                        }
                    }
                }
            }
        }
        private static byte[] getBytes_FromString(string s, int byteLength)
        {
            if (String.IsNullOrWhiteSpace(s.Trim()))
            {
                return getNullBytes(byteLength);
            }
            byte[] b1 = Encoding.GetEncoding("iso-8859-1").GetBytes(s);
            if (b1.Length > byteLength) throw new ArgumentException("String byte length greater than requested byte length.");
            byte[] b2 = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
                if (i < b1.Length)
                {
                    b2[i] = b1[i];
                }
                else
                {
                    b2[i] = (byte)0;
                }
            }
            return b2;
        }
        private static byte[] getNullBytes(int byteLength)
        {
            byte[] b = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
                b[i] = 0;
            }
            return b;
        }
        private static Int32 UnixTimeStampFromDateTime(DateTime dt)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dt = dt.ToUniversalTime();
            return (int)TimeSpan.FromTicks(dt.Ticks - dtDateTime.Ticks).TotalSeconds;
        }
        private void getData(bool ignore_C106_version)
        {
            this.Hdc = new Hdc_Struct();
            byte[] b = File.ReadAllBytes(this.cdf_File.FullName);
            using (BinaryReader reader = new BinaryReader(new MemoryStream(b)))
            {
                BinaryReader r = reader;

                //Hdc_Struct
                try
                {
                    this.Hdc._cdfver = getString(ref r, 6);
                    if (!ignore_C106_version)
                    {
                        if (this.Hdc._cdfver == "C106")
                        {
                            this.fileReadError = true;
                            this.errorMsg = "CDF version C106 not supported.";
                            return;
                        }
                        else if (this.Hdc._cdfver != "C106B")
                        {
                            this.fileReadError = true;
                            this.errorMsg = "Invalid CDF file version.";
                            return;
                        }
                    }
                    this.Hdc._operator = getString(ref r, 12);
                    this.Hdc._starttime = UnixTimeStampToDateTime(getInt32(ref r));
                    this.Hdc._finishtime = UnixTimeStampToDateTime(getInt32(ref r));
                    this.Hdc._mode = getString(ref r, 6);
                    this.Hdc._tstrtyp = getString(ref r, 6);
                    this.Hdc._tstrnum = getString(ref r, 6);
                    this.Hdc._statnum = getString(ref r, 6);
                    this.Hdc._flat = getString(ref r, 4);
                    this.Hdc._xcen = getInt16(ref r);
                    this.Hdc._ycen = getInt16(ref r);
                    this.Hdc._swtyp = getString(ref r, 14);
                    this.Hdc._swname = getString(ref r, 14);
                    this.Hdc._swver = getString(ref r, 6);
                    this.Hdc._swdate = getInt32(ref r);
                    this.Hdc._sitecode = getString(ref r, 12);
                    this.Hdc._partfmly = getString(ref r, 12);
                    this.Hdc._partdesig = getString(ref r, 12);
                    this.Hdc._xdim = getInt16(ref r);
                    this.Hdc._ydim = getInt16(ref r);
                    this.Hdc._bths = getInt16(ref r);
                    this.Hdc._crrs = getInt16(ref r);
                    this.Hdc._bdcs = getInt16(ref r);
                    this.Hdc._tdcs = getInt16(ref r);
                    this.Hdc._ddcs = getUInt16(ref r);
                    this.Hdc._trcs = getInt16(ref r);
                    this.Hdc._xlocmin = getInt16(ref r);
                    this.Hdc._xlocmax = getInt16(ref r);
                    this.Hdc._ylocmin = getInt16(ref r);
                    this.Hdc._ylocmax = getInt16(ref r);
                    this.Hdc._usvn = getString(ref r, 6);
                    this.Hdc._csvn = getString(ref r, 6);
                    this.Hdc._hdcs = getInt16(ref r);
                    this.Hdc._docs = getInt32(ref r);
                    this.Hdc._datalevel = getInt16(ref r);
                    this.Hdc._mainrver = getString(ref r, 6);
                    this.Hdc._mainrdate = UnixTimeStampToDateTime(getInt32(ref r));
                    this.Hdc._reserved = getString(ref r, 2);
                    this.Hdc._catlist = getString(ref r, 40);
                    this.Hdc._customer = getString(ref r, 40);
                    this.Hdc._workorder = getString(ref r, 40);
                    this.Hdc._datecode = getString(ref r, 6);
                    this.Hdc._Lot = getString(ref r, 16);
                    this.Hdc._Sublot = getString(ref r, 12);
                    this.Hdc._tstrdscr = getString(ref r, 14);
                    this.Hdc._devcnt2 = getInt32(ref r);
                    this.Hdc._future = getString(ref r, 74);
                    this.Hdc._chksum = getString(ref r, 8);

                    //Doc Struct
                    if (this.Hdc._docs > 0)
                    {
                        this.Doc = new Doc_Struct[this.Hdc._docs];
                        for (int i = 0; i < this.Hdc._docs; i++)
                        {
                            this.Doc[i] = new Doc_Struct();
                            this.Doc[i]._txt = getString(ref r, 82);
                        }
                    }

                    //Bth Struct
                    if (this.Hdc._bths > 0)
                    {
                        this.Bth = new Bth_Struct[this.Hdc._bths];
                        for (int i = 0; i < this.Hdc._bths; i++)
                        {
                            this.Bth[i] = new Bth_Struct();
                            this.Bth[i]._lot = getString(ref r, 16);
                            this.Bth[i]._sublot = getString(ref r, 12);
                            this.Bth[i]._future = getString(ref r, 16);
                        }
                    }

                    //Crr Struct
                    if (this.Hdc._crrs > 0)
                    {
                        this.Crr = new Crr_Struct[this.Hdc._crrs];
                        for (int i = 0; i < this.Hdc._crrs; i++)
                        {
                            this.Crr[i] = new Crr_Struct();
                            this.Crr[i]._partdesig = getString(ref r, 12);
                            this.Crr[i]._sernum = getString(ref r, 12);
                            this.Crr[i]._testdate = UnixTimeStampToDateTime(getInt32(ref r));
                            this.Crr[i]._testname = getString(ref r, 12);
                            this.Crr[i]._idealval = getFloat(ref r);
                            this.Crr[i]._actualval = getFloat(ref r);
                            this.Crr[i]._deltamax = getFloat(ref r);
                            this.Crr[i]._deltamin = getFloat(ref r);
                            this.Crr[i]._future = getString(ref r, 16);
                        }
                    }

                    //Bdc Struct
                    if (this.Hdc._bdcs > 0)
                    {
                        this.Bdc = new Bdc_Struct[this.Hdc._bdcs];
                        for (int i = 0; i < this.Hdc._bdcs; i++)
                        {
                            this.Bdc[i] = new Bdc_Struct();
                            this.Bdc[i]._binname = getString(ref r, 24);
                            this.Bdc[i]._binnum = getInt16(ref r);
                            this.Bdc[i]._bincnt = getInt32(ref r);
                            this.Bdc[i]._future = getString(ref r, 8);
                        }
                    }

                    //Tdc Struct
                    if (this.Hdc._tdcs > 0)
                    {
                        this.Tdc = new Tdc_Struct[this.Hdc._tdcs];
                        for (int i = 0; i < this.Hdc._tdcs; i++)
                        {
                            this.Tdc[i] = new Tdc_Struct();
                            this.Tdc[i]._testname = getString(ref r, 24);
                            this.Tdc[i]._statname = getString(ref r, 12);
                            this.Tdc[i]._testnum = getInt16(ref r);
                            this.Tdc[i]._units = getString(ref r, 6);
                            this.Tdc[i]._rangelo = getFloat(ref r);
                            this.Tdc[i]._rangehi = getFloat(ref r);
                            this.Tdc[i]._resolution = getFloat(ref r);
                            this.Tdc[i]._lolimit = getFloat(ref r);
                            this.Tdc[i]._hilimit = getFloat(ref r);
                            this.Tdc[i]._anomfltrlo = getFloat(ref r);
                            this.Tdc[i]._anomfltrhi = getFloat(ref r);
                            this.Tdc[i]._execcnt = getInt32(ref r);
                            this.Tdc[i]._failcnt = getInt32(ref r);
                            this.Tdc[i]._minval = getFloat(ref r);
                            this.Tdc[i]._maxval = getFloat(ref r);
                            this.Tdc[i]._mean = getFloat(ref r);
                            this.Tdc[i]._sigma = getFloat(ref r);
                            this.Tdc[i]._skew = getFloat(ref r);
                            this.Tdc[i]._kurt = getFloat(ref r);
                            this.Tdc[i]._anomlo = getInt16(ref r);
                            this.Tdc[i]._anomhi = getInt16(ref r);
                            this.Tdc[i]._cvrgcnt = getInt16(ref r);
                            this.Tdc[i]._statflg = getInt32(ref r);
                            this.Tdc[i]._failcnt2 = getInt32(ref r);
                            this.Tdc[i]._datatype = getString(ref r, 2);
                            this.Tdc[i]._datasize = getInt16(ref r);
                            this.Tdc[i]._statcnt = getInt32(ref r);
                            this.Tdc[i]._median = getFloat(ref r);
                            this.Tdc[i]._uhng = getFloat(ref r);
                            this.Tdc[i]._lhng = getFloat(ref r);
                            this.Tdc[i]._uwsk = getFloat(ref r);
                            this.Tdc[i]._lwsk = getFloat(ref r);
                            this.Tdc[i]._uifnc = getFloat(ref r);
                            this.Tdc[i]._lifnc = getFloat(ref r);
                            this.Tdc[i]._uofnc = getFloat(ref r);
                            this.Tdc[i]._lofnc = getFloat(ref r);
                            this.Tdc[i]._future = getString(ref r, 20);
                        }
                    }

                    //Ddc Struct
                    if (this.Hdc._ddcs > 0)
                    {
                        int count = this.Hdc._ddcs;
                        if (this.Hdc._devcnt2 > 0)
                            count = this.Hdc._devcnt2;
                        this.Ddc = new Ddc_Struct[count];
                        for (int i = 0; i < count; i++)
                        {
                            this.Ddc[i] = new Ddc_Struct();
                            this.Ddc[i]._dienum = getInt32(ref r);
                            this.Ddc[i]._xpos = getInt16(ref r);
                            this.Ddc[i]._ypos = getInt16(ref r);
                            this.Ddc[i]._bincode = getInt16(ref r);
                            if (this.Hdc._trcs > 0)
                            {
                                this.Ddc[i].Trc = new Trc_Struct[this.Hdc._trcs];
                                for (int j = 0; j < this.Hdc._trcs; j++)
                                {
                                    this.Ddc[i].Trc[j]._datatype = this.Tdc[j]._datatype;
                                    this.Ddc[i].Trc[j]._result = reader.ReadBytes(this.Tdc[j]._datasize);
                                    this.Ddc[i].Trc[j]._floatValue = getFloatFrom_Trc_result(this.Ddc[i].Trc[j]._result, this.Tdc[j]._rangelo, this.Tdc[j]._rangehi, this.Tdc[j]._resolution);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.fileReadError = true;
                    this.errorMsg = "Error reading CDF file.  " + ex.Message;
                }
            }
        }
        private float getFloatFrom_Trc_result(byte[] result, float rangelo, float rangehi, float resolution)
        {
            Int32 iFlag, iMagnitude, rVal;
            if (result.Length == 4)
                rVal = BitConverter.ToInt32(result, 0);
            else
                rVal = BitConverter.ToInt16(result, 0);
            iFlag = rVal & 0xC000;
            if (iFlag != 0) return Single.NaN;
            iMagnitude = rVal & 0x3FFF;
            float fVal = ((iMagnitude * (rangehi - rangelo)) / resolution) + rangelo;
            return fVal;
        }
        private string getString(ref BinaryReader reader, int numBytes)
        {
            return Encoding.GetEncoding("iso-8859-1").GetString(reader.ReadBytes(numBytes)).Replace("\0", string.Empty);
        }
        private UInt16 getUInt16(ref BinaryReader reader)
        {
            return BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        }
        private Int32 getInt32(ref BinaryReader reader)
        {
            return BitConverter.ToInt32(reader.ReadBytes(4), 0);
        }
        private Int16 getInt16(ref BinaryReader reader)
        {
            return BitConverter.ToInt16(reader.ReadBytes(2), 0);
        }
        private float getFloat(ref BinaryReader reader)
        {
            return BitConverter.ToSingle(reader.ReadBytes(4), 0);
        }
        public struct Hdc_Struct
        {
            public string _cdfver;
            public string _operator;
            public DateTime _starttime;
            public DateTime _finishtime;
            public string _mode;
            public string _tstrtyp;
            public string _tstrnum;
            public string _statnum;
            public string _flat;
            public Int16 _xcen;
            public Int16 _ycen;
            public string _swtyp;
            public string _swname;
            public string _swver;
            public Int32 _swdate;
            public string _sitecode;
            public string _partfmly;
            public string _partdesig;
            public Int16 _xdim;
            public Int16 _ydim;
            public Int16 _bths;
            public Int16 _crrs;
            public Int16 _bdcs;
            public Int16 _tdcs;
            public UInt16 _ddcs;
            public Int16 _trcs;
            public Int16 _xlocmin;
            public Int16 _xlocmax;
            public Int16 _ylocmin;
            public Int16 _ylocmax;
            public string _usvn;
            public string _csvn;
            public Int16 _hdcs;
            public Int32 _docs;
            public Int16 _datalevel;
            public string _mainrver;
            public DateTime _mainrdate;
            public string _reserved;
            public string _catlist;
            public string _customer;
            public string _workorder;
            public string _datecode;
            public string _Lot;
            public string _Sublot;
            public string _tstrdscr;
            public Int32 _devcnt2;
            public string _future;
            public string _chksum;
        }
        public struct Doc_Struct
        {
            public string _txt;
        }
        public struct Bth_Struct
        {
            public string _lot;
            public string _sublot;
            public string _future;
        }
        public struct Crr_Struct
        {
            public string _partdesig;
            public string _sernum;
            public DateTime _testdate;
            public string _testname;
            public float _idealval;
            public float _actualval;
            public float _deltamax;
            public float _deltamin;
            public string _future;
        }
        public struct Bdc_Struct
        {
            public string _binname;
            public Int16 _binnum;
            public Int32 _bincnt;
            public string _future;
        }
        public struct Tdc_Struct
        {
            public string _testname;
            public string _statname;
            public Int16 _testnum;
            public string _units;
            public float _rangelo;
            public float _rangehi;
            public float _resolution;
            public float _lolimit;
            public float _hilimit;
            public float _anomfltrlo;
            public float _anomfltrhi;
            public Int32 _execcnt;
            public Int32 _failcnt;
            public float _minval;
            public float _maxval;
            public float _mean;
            public float _sigma;
            public float _skew;
            public float _kurt;
            public Int16 _anomlo;
            public Int16 _anomhi;
            public Int16 _cvrgcnt;
            public Int32 _statflg;
            public Int32 _failcnt2;
            public string _datatype;
            public Int16 _datasize;
            public Int32 _statcnt;
            public float _median;
            public float _uhng;
            public float _lhng;
            public float _uwsk;
            public float _lwsk;
            public float _uifnc;
            public float _lifnc;
            public float _uofnc;
            public float _lofnc;
            public string _future;
        }
        public struct Ddc_Struct
        {
            public Int32 _dienum;
            public Int16 _xpos;
            public Int16 _ypos;
            public Int16 _bincode;
            public Trc_Struct[] Trc;
        }
        public struct Trc_Struct
        {
            public string _datatype;
            public byte[] _result;
            public float _floatValue;
        }
    }
}
