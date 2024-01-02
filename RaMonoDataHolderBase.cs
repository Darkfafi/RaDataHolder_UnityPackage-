﻿using System;
using UnityEngine;

namespace RaDataHolder
{
	public abstract class RaMonoDataHolderBase<TData> : MonoBehaviour, IRaDataHolder<TData>
	{
		public delegate void DataHandler(RaMonoDataHolderBase<TData> holder);
		public delegate void DataChangeHandler(TData newData, TData oldData, RaMonoDataHolderBase<TData> core);

		public event DataHandler DataSetEvent;
		public event DataHandler DataClearedEvent;
		public event DataHandler DataSetResolvedEvent;
		public event DataHandler DataClearResolvedEvent;
		public event DataChangeHandler DataReplacedEvent;

		private bool _isDestroyed = false;

		public bool HasData => _core != null && _core.HasData;

		public bool IsReplacingData => _core != null && _core.IsReplacingData;

		protected RaDataHolderCore<TData> Core
		{
			get
			{
				TryInitialize();
				return _core;
			}
		}
		private RaDataHolderCore<TData> _core = null;

		protected TData Data
		{
			get; private set;
		}

		protected void Awake()
		{
			TryInitialize();
		}

		protected void OnDestroy()
		{
			if(_isDestroyed)
			{
				return;
			}

			_isDestroyed = true;

			DataSetEvent = null;
			DataClearedEvent = null;
			DataSetResolvedEvent = null;
			DataClearResolvedEvent = null;
			DataReplacedEvent = null;

			_core.ClearData(true);
			
			OnDeinitialization();
			
			_core.Dispose();
			_core = null;

			Data = default;
		}

		private void TryInitialize()
		{
			if(_isDestroyed || _core != null)
			{
				return;
			}

			_core = new RaDataHolderCore<TData>(
			(core) =>
			{
				Data = core.Data;
				OnSetData();
			},
			(core) =>
			{
				OnClearData();
				Data = default;
			},
			(core)=> 
			{
				OnSetDataResolved();
			}, 
			(core)=> 
			{
				OnClearDataResolved();
			});

			_core.DataSetEvent += OnDataSetEvent;
			_core.DataClearedEvent += OnDataClearedEvent;
			_core.DataSetResolvedEvent += OnDataSetResolvedEvent;
			_core.DataClearResolvedEvent += OnDataClearResolvedEvent;
			_core.DataReplacedEvent += OnDataReplacedEvent;

			OnInitialization();
		}

		public void EditorSetData(TData data)
		{
			SetData(data);
		}

		public void EditorClearData()
		{
			ClearData();
		}

		public void EditorReplaceData(TData data)
		{
			ReplaceData(data, ignoreOnEqual: false);
		}

		public IRaDataSetResolver SetData(TData data, bool resolve = true)
		{
			if(Core != null)
			{
				Core.SetData(data, resolve);
			}
			return this;
		}

		public IRaDataSetResolver SetRawData(object data, bool resolve = true)
		{
			if(Core != null)
			{
				Core.SetRawData(data, resolve);
			}
			return this;
		}

		public IRaDataClearResolver ClearData(bool resolve = true)
		{
			if(Core != null)
			{
				Core.ClearData(resolve);
			}
			return this;
		}

		public void ReplaceData(TData data, bool ignoreOnEqual = true)
		{
			if(Core != null)
			{
				Core.ReplaceData(data, ignoreOnEqual);
			}
		}

		protected abstract void OnSetData();
		
		protected abstract void OnClearData();

		protected virtual void OnSetDataResolved()
		{

		}

		protected virtual void OnClearDataResolved()
		{

		}

		protected virtual void OnInitialization()
		{

		}

		protected virtual void OnDeinitialization()
		{

		}

		private void OnDataSetEvent(RaDataHolderCore<TData> core)
		{
			DataSetEvent?.Invoke(this);
		}

		private void OnDataClearedEvent(RaDataHolderCore<TData> core)
		{
			DataClearedEvent?.Invoke(this);
		}

		private void OnDataSetResolvedEvent(RaDataHolderCore<TData> core)
		{
			DataSetResolvedEvent?.Invoke(this);
		}

		private void OnDataClearResolvedEvent(RaDataHolderCore<TData> core)
		{
			DataClearResolvedEvent?.Invoke(this);
		}

		private void OnDataReplacedEvent(TData newData, TData oldData, RaDataHolderCore<TData> core)
		{
			DataReplacedEvent?.Invoke(newData, oldData, this);
		}

		public IRaDataSetResolver Resolve()
		{
			((IRaDataSetResolver)Core).Resolve();
			return this;
		}

		IRaDataClearResolver IRaDataClearResolver.Resolve()
		{
			((IRaDataClearResolver)Core).Resolve();
			return this;
		}
	}
}
