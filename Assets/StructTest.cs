using System.Collections.Generic;
using System.Linq;

struct Enumerable<T>
{
    public struct Enumerator
    {
        int index;
        readonly T[] list;
        public Enumerator(T[] list)
        {
            this.list = list;
            index = -1;
        }
        
        public T Current => list[index];
        public bool MoveNext() => list.Length > (++index);
    }
    readonly T[] list;
    public Enumerable(T[] list) => this.list = list;
    public Enumerator GetEnumerator() => new Enumerator(list);

}

public static class Program
{
    public static void Main()
    {
        var nums = new[] { 0, 1, 2 };
        // local
        var e = new Enumerable<int>(nums);
        foreach (var item in e);
        // immediate        
        foreach (var item in new Enumerable<int>(nums));
    }

	static Stack<object> stack = new Stack<object>();
	public static object Shark()
	{
		foreach(var s in stack)
		{
			return s;
		}
		return null;
	}
	public static object Snake() => stack.First();
}