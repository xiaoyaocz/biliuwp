﻿using System;
using System.Threading;
using Windows.UI.Xaml.Controls;

namespace BiliBili.UWP.Helper
{
	public class AysncUnhandledExceptionEventArgs : EventArgs
	{
		public Exception Exception { get; set; }
		public bool Handled { get; set; }
	}

	internal class ExceptionHandlingSynchronizationContext : SynchronizationContext
	{
		private readonly SynchronizationContext _syncContext;

		public ExceptionHandlingSynchronizationContext(SynchronizationContext syncContext)
		{
			_syncContext = syncContext;
		}

		public event EventHandler<AysncUnhandledExceptionEventArgs> UnhandledException;

		/// <summary>
		/// 注册事件. 需要在OnLaunched和OnActivated事件中调用
		/// </summary>
		/// <returns></returns>
		public static ExceptionHandlingSynchronizationContext Register()
		{
			var syncContext = Current;
			if (syncContext == null)
				throw new InvalidOperationException("Ensure a synchronization context exists before calling this method.");

			var customSynchronizationContext = syncContext as ExceptionHandlingSynchronizationContext;

			if (customSynchronizationContext == null)
			{
				customSynchronizationContext = new ExceptionHandlingSynchronizationContext(syncContext);
				SetSynchronizationContext(customSynchronizationContext);
			}

			return customSynchronizationContext;
		}

		/// <summary>
		/// 将线程的上下文绑定到特定的Frame上面
		/// </summary>
		/// <param name="rootFrame"></param>
		/// <returns></returns>
		public static ExceptionHandlingSynchronizationContext RegisterForFrame(Frame rootFrame)
		{
			if (rootFrame == null)
				throw new ArgumentNullException("rootFrame");

			var synchronizationContext = Register();

			rootFrame.Navigating += (sender, args) => EnsureContext(synchronizationContext);
			rootFrame.Loaded += (sender, args) => EnsureContext(synchronizationContext);

			return synchronizationContext;
		}

		public override SynchronizationContext CreateCopy()
		{
			return new ExceptionHandlingSynchronizationContext(_syncContext.CreateCopy());
		}

		public override void OperationCompleted()
		{
			_syncContext.OperationCompleted();
		}

		public override void OperationStarted()
		{
			_syncContext.OperationStarted();
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			_syncContext.Post(WrapCallback(d), state);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			_syncContext.Send(d, state);
		}

		private static void EnsureContext(SynchronizationContext context)
		{
			if (Current != context)
				SetSynchronizationContext(context);
		}

		private bool HandleException(Exception exception)
		{
			if (UnhandledException == null)
				return false;

			var exWrapper = new AysncUnhandledExceptionEventArgs
			{
				Exception = exception
			};

			UnhandledException(this, exWrapper);

#if DEBUG && !DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
			return exWrapper.Handled;
		}

		private SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback)
		{
			return state =>
			{
				try
				{
					sendOrPostCallback(state);
				}
				catch (Exception ex)
				{
					if (!HandleException(ex))
						throw;
				}
			};
		}
	}
}