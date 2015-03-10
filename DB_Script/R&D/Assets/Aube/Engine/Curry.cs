using System;

namespace Aube
{
	//! @class	Curry
	//!
	//! @brief	Class that provides static methods to bind arguments into a function signature.
	public class Curry
	{
	//*********************************************************************
	// Functions
	//*********************************************************************
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<TResult> Bind<T1, TResult>(Func<T1, TResult> a_func, T1 a_arg1)
		{
			return () => a_func(a_arg1);
		}

		public static Action Bind<T1>(Action<T1> a_func, T1 a_arg1)
		{
			return () => a_func(a_arg1);
		}
		
		//*********************************************************************
		// Bind first argument
		//*********************************************************************
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T2, TResult> BindFirst<T1, T2, TResult>(Func<T1, T2, TResult> a_func, T1 a_arg1)
		{
			return (arg2) => a_func(a_arg1, arg2);
		}
		
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T2, T3, TResult> BindFirst<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> a_func, T1 a_arg1)
		{
			return (arg2, arg3) => a_func(a_arg1, arg2, arg3);
		}
		
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T2, T3, T4, TResult> BindFirst<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> a_func, T1 a_arg1)
		{
			return (arg2, arg3, arg4) => a_func(a_arg1, arg2, arg3, arg4);
		}
		
		//*********************************************************************
		// Bind last argument
		//*********************************************************************
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T1, TResult> BindLast<T1, T2, TResult>(Func<T1, T2, TResult> a_func, T2 a_arg2)
		{
			return (arg1) => a_func(arg1, a_arg2);
		}
		
		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T1, T2, TResult> BindLast<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> a_func, T3 a_arg3)
		{
			return (arg1, arg2) => a_func(arg1, arg2, a_arg3);
		}

		///////////////////////////////////////////////////////////////////////
		//
		//
		///////////////////////////////////////////////////////////////////////
		public static Func<T1, T2, T3, TResult> BindLast<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> a_func, T4 a_arg4)
		{
			return (arg1, arg2, arg3) => a_func(arg1, arg2, arg3, a_arg4);
		}
	}
} // namespace Aube