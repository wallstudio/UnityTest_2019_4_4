package world.hello
import java.io.File
import android.app.Activity
import android.content.ActivityNotFoundException
import android.content.Context
import android.content.Intent
import androidx.core.content.FileProvider


fun getText(context: Context, postfix: String, value: Int) : String
{
	return "${context}${value}${postfix}"
}

fun openFile(context: Context, path: String, mime: String) : String
{
	try
	{
		// Android7.x～はこの方法でないと開けない
		// https://github.com/Mercandj/file-android/blob/7455f68f85775c41aee5fbaa59b5beb7fcd954e1/file_api_android/src/main/java/com/mercandalli/android/sdk/files/api/FileModule.kt#L110
		val uri = FileProvider.getUriForFile(context, context.applicationContext.packageName + ".fileprovider", File(path))
		val intent = Intent()
		intent.action = Intent.ACTION_VIEW
		intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
		intent.setDataAndType(uri, mime)
		if (context !is Activity)
		{
			intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
		}
		intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
		context.startActivity(intent)
		return uri.toString()
	}
	catch (e: Exception)
	{
		return e.toString()
	}
}
