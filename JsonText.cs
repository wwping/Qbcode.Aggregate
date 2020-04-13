using BeetleX.Buffers;
using BeetleX.FastHttpApi;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Qbcode.Aggregate
{
    public class JsonText : ResultBase
    {
        public JsonText(StringBuilder text)
        {
            this.mText = text;
        }

        public override IHeaderItem ContentType
        {
            get
            {
                return ContentTypes.JSON;
            }
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            char[] array = ArrayPool<char>.Shared.Rent(this.mText.Length);
            byte[] array2 = ArrayPool<byte>.Shared.Rent(this.mText.Length * 6);
            try
            {
                this.mText.CopyTo(0, array, 0, this.mText.Length);
                int bytes = Encoding.UTF8.GetBytes(array, 0, this.mText.Length, array2, 0);
                stream.Write(array2, 0, bytes);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(array, false);
                ArrayPool<byte>.Shared.Return(array2, false);
            }
        }

        private StringBuilder mText;
    }
}
