using osu.Framework.Platform;

namespace osu.XR.IO;

public static class StorageExtensions {
	public static Stream WriteWithBackup ( this Storage storage, string fileName ) {
		var temp = fileName + "~";

		return new SafeStream( storage.GetStream( temp, FileAccess.Write, FileMode.Create ), storage, temp, fileName );
	}

	public static Stream? ReadWithBackup ( this Storage storage, string fileName ) {
		var backup = fileName + "~";

		if ( storage.Exists( fileName ) ) {
			return storage.GetStream( fileName, FileAccess.Read, FileMode.Open );
		}
		else if ( storage.Exists( backup ) ) {
			return storage.GetStream( backup, FileAccess.Read, FileMode.Open );
		}
		return null;
	}

	class SafeStream : Stream {
		Stream source;
		Storage storage;
		string temp; 
		string final;
		public SafeStream ( Stream source, Storage storage, string temp, string final ) {
			this.source = source;
			this.storage = storage;
			this.temp = temp;
			this.final = final;
		}

		bool isDisposed;
		protected override void Dispose ( bool disposing ) {
			if ( isDisposed )
				return;

			isDisposed = true;
			source.Dispose();
			if ( storage.Exists( final ) )
				storage.Delete( final );
			storage.Move( temp, final );
		}

		public override void Flush () {
			source.Flush();
		}

		public override int Read ( byte[] buffer, int offset, int count ) {
			return source.Read( buffer, offset, count );
		}

		public override long Seek ( long offset, SeekOrigin origin ) {
			return source.Seek( offset, origin );
		}

		public override void SetLength ( long value ) {
			source.SetLength( value );
		}

		public override void Write ( byte[] buffer, int offset, int count ) {
			source.Write( buffer, offset, count );
		}

		public override bool CanRead => source.CanRead;

		public override bool CanSeek => source.CanSeek;

		public override bool CanWrite => source.CanWrite;

		public override long Length => source.Length;

		public override long Position { get => source.Position; set => source.Position = value; }
	}
}
