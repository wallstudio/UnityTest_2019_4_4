package world.hello
import java.io.File
import android.app.Activity
import android.content.ActivityNotFoundException
import android.content.Context
import android.content.Intent


fun getText(context: Context, postfix: String, value: Int) : String
{
	return "${context}${value}${postfix}"
}
