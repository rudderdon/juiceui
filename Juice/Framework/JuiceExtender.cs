﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Juice.Framework {

	public abstract class JuiceExtender : ExtenderControl, IWidget, IPostBackDataHandler, IPostBackEventHandler {

		private Control _targetControl;
		private JuiceWidgetState _widgetState;
		private String _widgetName;

		protected JuiceExtender(String widgetName) {
			if(string.IsNullOrEmpty(widgetName)) {
				throw new ArgumentException("The parameter must not be empty", "widgetName");
			}

			_widgetName = widgetName;
			_widgetState = new JuiceWidgetState(this);

			SetDefaultOptions();
		}

		[Browsable(false)]
		private JuiceWidgetState WidgetState { get { return this._widgetState; } }

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			_targetControl = FindControl(TargetControlID);

			if(_targetControl == null) {
				throw new ArgumentNullException("TargetControl is null");
			}

			WidgetState.SetWidgetNameOnTarget(_targetControl as IAttributeAccessor);
			WidgetState.AddPagePreRenderCompleteHandler();
		}

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if(_targetControl.Visible) {
				Page.RegisterRequiresPostBack(this);
				WidgetState.ParseEverything(_targetControl);
				WidgetState.RenderCss();
			}
		}

		protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
			WidgetState.LoadPostData();
			return false;
		}

		protected virtual void RaisePostDataChangedEvent() { }

		protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors(Control targetControl) {
			return null;
		}

		protected override IEnumerable<ScriptReference> GetScriptReferences() {
			return WidgetState.GetJuiceReferences();
		}

		protected virtual IDictionary<string, object> SaveOptionsAsDictionary() {
			return WidgetState.ParseOptions();
		}

		protected virtual void SetDefaultOptions() {
			WidgetState.SetDefaultOptions();
		}

		#region IWidget Implementation

		/// <summary>
		/// Disables (true) or enables (false) the widget.
		/// </summary>
		[WidgetOption("disabled", false)] // every widget has a disabled option.
		[Browsable(false)]
		[Category("Behavior")]
		[DefaultValue(false)]
		[Description("Disables (true) or enables (false) the widget.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Disabled {
			get {
				return (bool)(ViewState["Disabled"] ?? false);
			}
			set {
				ViewState["Disabled"] = value;
			}
		}

		/// <summary>
		/// True, if the control should automatically postback to the server after the selected value is changed. False, otherwise.
		/// </summary>
		[DefaultValue(false)]
		[Description("True, if the control should automatically postback to the server after the selected value is changed. False, otherwise.")]
		[Category("Behavior")]
		public bool AutoPostBack {
			get {
				return (bool)(ViewState["AutoPostBack"] ?? false);
			}
			set {
				ViewState["AutoPostBack"] = value;
			}
		}
		
		/// <summary>
		/// The jQuery UI name of the widget.
		/// </summary>
		[Browsable(false)]
		public string WidgetName { get { return this._widgetName; } }
		
		Page IWidget.Page { get { return Page; } }

		string IWidget.ClientID { get { return ClientID; } }

		string IWidget.UniqueID { get { return UniqueID; } }

		bool IWidget.Visible { get { return Visible; } }

		void IWidget.SaveWidgetOptions() {
			((IWidget)this).WidgetOptions = SaveOptionsAsDictionary();
		}

		IDictionary<string, object> IWidget.WidgetOptions { get; set; }

		#endregion

		#region IPostBackDataHandler implementation

		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
			return LoadPostData(postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent() {
			RaisePostDataChangedEvent();
		}

		#endregion

		#region IPostBackEventHandler Implementation

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
			WidgetState.RaisePostBackEvent(eventArgument);
		}

		#endregion

	}
}