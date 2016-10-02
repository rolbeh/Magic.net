﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceModel.Channels;
using JetBrains.Annotations;

namespace Magic.Net
{
    internal abstract class ReadWriteStreamAdapter :  INetConnectionAdapter
    {
        #region Fields
        private readonly byte[] _lenBuffer = new byte[sizeof(int)];

        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.
        private readonly Stream _stream;
        private readonly BufferManager _bufferManager;

        #endregion Fields

        protected ReadWriteStreamAdapter([NotNull]Stream stream,[NotNull] BufferManager bufferManager)
        {
            this._stream = stream;
            this._bufferManager = bufferManager;
        }

        #region Implementation of INetConnectionAdapter

        public abstract bool IsConnected { get; }
        public Uri RemoteAddress { get; protected set; }
        
        public abstract void Open();

        public virtual void Close()
        {
            _stream.Close();
        }

        [CanBeNull]
        public NetDataPackage ReadData()
        {
            //Länge der zu empfangenden Daten lesen 4 bytes = int32
            var rlen = _stream.Read(_lenBuffer, 0, 4);
            if (0 == rlen)
            {
                // Keine Daten, die Pipe wurde geschlossen oder irgendwas lief schief
                Dispose();
                return null;
            }


            var _len = BitConverter.ToInt32(_lenBuffer, 0);

            //eigentliche Daten lesen
            // Die Daten können aus mehreren Segmenten bestehen, der muss auf die Reihenfolge achten
            var currentCount = 0;
            var bytes1 = new byte[_len];
            while (currentCount < _len)
            {
                rlen = _stream.Read(bytes1, currentCount, _len - currentCount);
                currentCount += rlen;
                if (rlen == 0)
                {
                    // Keine Daten, die Pipe wurde geschlossen oder irgendwas lief schief
                    Dispose();
                    return null;
                }
            }

            //Debug.WriteLine("ReceiveFromStreamInternal " + (bytes1.Length + 4));
            var p = new NetDataPackage(bytes1);
            return  p;
        }

        public void WriteData(params ArraySegment<byte>[] buffers)
        {
            // ReSharper disable once BuiltInTypeReferenceStyle
            Int32 len = buffers.Sum(b => b.Count);
            byte[] lenBuffer = _bufferManager.TakeBuffer(4);

            len.ToBuffer(lenBuffer);
            _stream.Write(lenBuffer, 0, 4);
            _bufferManager.ReturnBuffer(lenBuffer);

            foreach (ArraySegment<byte> arraySegment in buffers)
            {
                _stream.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            }

            _stream.Flush();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~ReadWriteStreamAdapter() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}