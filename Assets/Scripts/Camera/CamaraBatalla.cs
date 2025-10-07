using System;
using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema de control de cámaras cinematográficas para combate.
    /// Maneja el enfoque automático en diferentes áreas del campo de batalla
    /// con soporte para Cinemachine 2 y 3 mediante reflexión.
    /// </summary>
    public class CamaraBatalla : MonoBehaviour
    {
        [Header("Virtual Cameras")]
        [Tooltip("Cámara principal - asignar CinemachineCamera (CM3) o CinemachineVirtualCamera (CM2)")]
        public Component vcamAliado;
        
        [Tooltip("Cámara para enemigos (opcional si usarUnaSolaCam = true)")]
        public Component vcamEnemigos;
        
        [Tooltip("Cámara para vista central (opcional si usarUnaSolaCam = true)")]
        public Component vcamCentro;

        [Header("Camera Mode")]
        [Tooltip("Si está activo, usa solo vcamAliado para todas las transiciones")]
        public bool usarUnaSolaCam = true;

        [Header("Orthographic Size Settings")]
        public float sizeCentro = 4.5f;
        public float sizeAliado = 4.0f;
        public float sizeEnemigos = 4.5f;
        
        [Tooltip("Calibra los tamaños basándose en Camera.main al iniciar")]
        public bool calibrarDesdeCamaraPrincipal = true;

        [Header("Priority Settings (Multi-Camera Mode)")]
        public int priAlta = 20;
        public int priBaja = 10;

        [Header("Fixed Camera")]
        [Tooltip("Bloquea el movimiento de cámara en una posición fija")]
        public bool bloquearMovimiento = false;
        
        [Tooltip("Punto de anclaje para cámara fija (usa origin si no se asigna)")]
        public Transform fixedAnchor;

        // Objeto dummy para controlar el target de las cámaras
        private Transform _dummy;

        private void Awake()
        {
            EnsureDummy();
        }

        private void Start()
        {
            ConfigurarTamañosIniciales();
            AplicarTamañosACamaras();
            
            if (Application.isPlaying) 
                AplicarBloqueo();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                AplicarBloqueo();
        }

        /// <summary>
        /// Configura los tamaños de cámara basándose en Camera.main si está habilitado.
        /// </summary>
        private void ConfigurarTamañosIniciales()
        {
            if (!calibrarDesdeCamaraPrincipal || Camera.main == null || !Camera.main.orthographic)
                return;

            float baseSize = Camera.main.orthographicSize;
            sizeCentro = baseSize;
            sizeEnemigos = baseSize;
            sizeAliado = Mathf.Max(0.1f, baseSize - 0.5f);
        }

        /// <summary>
        /// Aplica los tamaños configurados a todas las cámaras virtuales.
        /// </summary>
        private void AplicarTamañosACamaras()
        {
            SetOrthoSize(vcamAliado, sizeAliado);
            SetOrthoSize(vcamEnemigos, sizeEnemigos);
            SetOrthoSize(vcamCentro, sizeCentro);
        }

        #region Public API

        /// <summary>
        /// Activa o desactiva el bloqueo de movimiento de cámara.
        /// </summary>
        public void SetBloquearMovimiento(bool activar)
        {
            bloquearMovimiento = activar;
            if (Application.isPlaying) 
                AplicarBloqueo();
        }

        /// <summary>
        /// Enfoca la cámara en un slot de aliado específico.
        /// </summary>
        public void FocusAllySlot(Transform slotJugador)
        {
            if (bloquearMovimiento || slotJugador == null) 
                return;

            ConfigurarCamara(slotJugador.position, FocusKind.Aliado, sizeAliado);
        }

        /// <summary>
        /// Enfoca la cámara en un slot de enemigo específico.
        /// </summary>
        public void FocusEnemiesSlot(Transform slotEnemigo)
        {
            if (bloquearMovimiento || slotEnemigo == null) 
                return;

            ConfigurarCamara(slotEnemigo.position, FocusKind.Enemigos, sizeEnemigos);
        }

        /// <summary>
        /// Enfoca la cámara en el centro del campo de batalla.
        /// </summary>
        public void FocusCenterZero()
        {
            if (bloquearMovimiento) 
                return;

            ConfigurarCamara(Vector3.zero, FocusKind.Centro, sizeCentro);
        }

        #endregion

        #region Private Implementation

        private enum FocusKind { Aliado, Enemigos, Centro }

        /// <summary>
        /// Configura una cámara para enfocar en una posición específica.
        /// </summary>
        private void ConfigurarCamara(Vector3 posicion, FocusKind tipo, float tamaño)
        {
            EnsureDummy();
            _dummy.position = posicion;

            var cam = GetCam(tipo);
            SetFollow(cam, _dummy);
            SetOrthoSize(cam, tamaño);
            SetPrioridades(tipo);
        }

        /// <summary>
        /// Aplica el comportamiento de cámara fija cuando está activado.
        /// </summary>
        private void AplicarBloqueo()
        {
            if (!bloquearMovimiento) 
                return;

            EnsureDummy();
            Vector3 posicionFija = fixedAnchor != null ? fixedAnchor.position : Vector3.zero;
            _dummy.position = posicionFija;

            var cam = GetCam(FocusKind.Centro);
            SetFollow(cam, _dummy);
            SetOrthoSize(cam, sizeCentro);

            if (!usarUnaSolaCam)
            {
                SetPrioritySafe(vcamAliado, priBaja);
                SetPrioritySafe(vcamEnemigos, priBaja);
                SetPrioritySafe(vcamCentro, priAlta);
            }
        }

        /// <summary>
        /// Obtiene la cámara virtual apropiada según el tipo de enfoque.
        /// </summary>
        private Component GetCam(FocusKind kind)
        {
            if (usarUnaSolaCam || kind == FocusKind.Aliado)
                return vcamAliado;

            return kind switch
            {
                FocusKind.Enemigos => vcamEnemigos ?? vcamAliado,
                FocusKind.Centro => vcamCentro ?? vcamAliado,
                _ => vcamAliado
            };
        }

        /// <summary>
        /// Establece las prioridades de las cámaras virtuales en modo multi-cámara.
        /// </summary>
        private void SetPrioridades(FocusKind kind)
        {
            if (usarUnaSolaCam) 
                return;

            switch (kind)
            {
                case FocusKind.Aliado:
                    SetPrioritySafe(vcamAliado, priAlta);
                    SetPrioritySafe(vcamEnemigos, priBaja);
                    SetPrioritySafe(vcamCentro, priBaja);
                    break;
                    
                case FocusKind.Enemigos:
                    SetPrioritySafe(vcamAliado, priBaja);
                    SetPrioritySafe(vcamEnemigos, priAlta);
                    SetPrioritySafe(vcamCentro, priBaja);
                    break;
                    
                case FocusKind.Centro:
                    SetPrioritySafe(vcamAliado, priBaja);
                    SetPrioritySafe(vcamEnemigos, priBaja);
                    SetPrioritySafe(vcamCentro, priAlta);
                    break;
            }
        }

        /// <summary>
        /// Asegura que existe el objeto dummy para controlar el target de las cámaras.
        /// </summary>
        private void EnsureDummy()
        {
            if (_dummy != null) 
                return;

            const string DUMMY_NAME = "CM_FocusDummy";
            var existing = GameObject.Find(DUMMY_NAME);
            
            if (existing != null)
            {
                _dummy = existing.transform;
            }
            else
            {
                var dummyObject = new GameObject(DUMMY_NAME);
                dummyObject.hideFlags = Application.isPlaying 
                    ? HideFlags.DontSave | HideFlags.HideInHierarchy 
                    : HideFlags.DontSave;
                _dummy = dummyObject.transform;
            }
        }

        #endregion

        #region Cinemachine Reflection Helpers

        /// <summary>
        /// Establece el target de seguimiento de la cámara virtual usando reflexión.
        /// Compatible con Cinemachine 2 y 3.
        /// </summary>
        private void SetFollow(Component cam, Transform target)
        {
            if (cam == null || target == null) 
                return;

            var cameraType = cam.GetType();

            // Intenta propiedad Follow (CM2)
            var followProperty = cameraType.GetProperty("Follow");
            if (followProperty != null && followProperty.CanWrite)
            {
                followProperty.SetValue(cam, target);
                return;
            }

            // Intenta propiedad Target (CM3)
            var targetProperty = cameraType.GetProperty("Target");
            if (targetProperty != null)
            {
                var targetObj = targetProperty.GetValue(cam);
                if (targetObj != null)
                {
                    var trackedProperty = targetObj.GetType().GetProperty("TrackedObject");
                    if (trackedProperty != null && trackedProperty.CanWrite)
                        trackedProperty.SetValue(targetObj, target);
                }
            }
        }

        /// <summary>
        /// Establece la prioridad de una cámara virtual usando reflexión.
        /// </summary>
        private void SetPrioritySafe(Component cam, int priority)
        {
            if (cam == null) 
                return;

            var priorityProperty = cam.GetType().GetProperty("Priority");
            if (priorityProperty != null && priorityProperty.CanWrite)
                priorityProperty.SetValue(cam, priority);
        }

        /// <summary>
        /// Establece el tamaño ortográfico de una cámara virtual usando reflexión.
        /// Compatible con diferentes versiones de Cinemachine.
        /// </summary>
        private void SetOrthoSize(Component cam, float size)
        {
            if (cam == null) 
                return;

            var cameraType = cam.GetType();

            // Intenta acceso por propiedad Lens
            if (TrySetOrthoSizeViaProperty(cam, cameraType, size))
                return;

            // Intenta acceso por campo m_Lens
            TrySetOrthoSizeViaField(cam, cameraType, size);
        }

        /// <summary>
        /// Intenta establecer el tamaño ortográfico a través de la propiedad Lens.
        /// </summary>
        private bool TrySetOrthoSizeViaProperty(Component cam, Type cameraType, float size)
        {
            var lensProperty = cameraType.GetProperty("Lens");
            if (lensProperty == null || !lensProperty.CanRead || !lensProperty.CanWrite)
                return false;

            var lens = lensProperty.GetValue(cam);
            if (lens == null)
                return false;

            var orthoProperty = lens.GetType().GetProperty("OrthographicSize");
            if (orthoProperty == null || !orthoProperty.CanWrite)
                return false;

            orthoProperty.SetValue(lens, size);
            lensProperty.SetValue(cam, lens);
            return true;
        }

        /// <summary>
        /// Intenta establecer el tamaño ortográfico a través del campo m_Lens.
        /// </summary>
        private void TrySetOrthoSizeViaField(Component cam, Type cameraType, float size)
        {
            const System.Reflection.BindingFlags flags = 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic;

            var lensField = cameraType.GetField("m_Lens", flags);
            if (lensField == null)
                return;

            var lens = lensField.GetValue(cam);
            if (lens == null)
                return;

            var lensType = lens.GetType();
            
            // Intenta campo OrthographicSize
            var orthoField = lensType.GetField("OrthographicSize");
            if (orthoField != null)
            {
                orthoField.SetValue(lens, size);
                lensField.SetValue(cam, lens);
                return;
            }

            // Intenta propiedad OrthographicSize
            var orthoProperty = lensType.GetProperty("OrthographicSize");
            if (orthoProperty != null && orthoProperty.CanWrite)
            {
                orthoProperty.SetValue(lens, size);
                lensField.SetValue(cam, lens);
            }
        }

        #endregion
    }
}