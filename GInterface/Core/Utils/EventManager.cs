using GInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GInterface.Core.Utils
{
    /*
     * How To used
     * 
     *  // Instancia de EventManager
        EventManager eventManager = new EventManager();

        // Agregar un oyente para Event1
        eventManager.StartListening(EnumTypes.TransactionTask.Event1, () => Console.WriteLine("Event1 ha sido disparado"));

        // Agregar un oyente para Event2
        eventManager.StartListening(EnumTypes.TransactionTask.Event2, () => Console.WriteLine("Event2 ha sido disparado"));

        // Disparar Event1
        eventManager.TriggerEvent(EnumTypes.TransactionTask.Event1);

        // Disparar Event2
        eventManager.TriggerEvent(EnumTypes.TransactionTask.Event2);

        // Eliminar el oyente para Event1
        eventManager.StopListening(EnumTypes.TransactionTask.Event1, () => Console.WriteLine("Event1 ha sido disparado"));

        // Intentar disparar Event1 de nuevo (no debería hacer nada porque no hay oyentes)
        eventManager.TriggerEvent(EnumTypes.TransactionTask.Event1);
     * 
     */
    public class EventManager<T>
    {
        // Delegate for the event
        public delegate void EventListener(T eventData);

        private Dictionary<EnumTypes.TransactionTask, Action> eventDictionary;

        // Evento
        public event EventListener OnEvent;

        //Controller
        public EventManager()
        {
            eventDictionary = new Dictionary<EnumTypes.TransactionTask, Action>();
        }

        // Method for dispatch the event
        public void Dispatch(T eventData)
        {
            OnEvent?.Invoke(eventData);
        }

        public void StartListening(EnumTypes.TransactionTask enumType, Action listener)
        {
            if (eventDictionary.ContainsKey(enumType))
            {
                eventDictionary[enumType] += listener;
            }
            else
            {
                eventDictionary.Add(enumType, listener);
            }
        }

        public void StopListening(EnumTypes.TransactionTask enumType, Action listener)
        {
            if (eventDictionary.ContainsKey(enumType))
            {
                eventDictionary[enumType] -= listener;
            }
        }

        public void TriggerEvent(EnumTypes.TransactionTask enumType)
        {
            Action thisEvent;
            if (eventDictionary.TryGetValue(enumType, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }

    }
}
