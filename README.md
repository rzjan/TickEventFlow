# TickEventFlow

Proyecto de referencia sobre **Event Sourcing** y arquitectura orientada a eventos en **.NET 9**.  
El objetivo es mostrar cómo aplicar **DDD**, **CQRS** y patrones orientados a eventos en una API moderna, sirviendo como guía práctica y base reutilizable para proyectos reales.

---

## 📌 Descripción

TickEventFlow implementa un flujo completo de creación y gestión de tickets utilizando **eventos de dominio** y un **Event Store** persistido en MongoDB.  
Los eventos se publican en **Kafka** para su consumo por otros servicios, garantizando un diseño desacoplado y escalable.

---

## 🏗️ Arquitectura

- **API HTTP** con endpoint principal: `POST /api/tickets`  
- **MediatR** para el manejo de comandos  
- **AggregateRoot** que generan eventos de dominio (`TicketCreatedEvent`, `TicketUpdatedEvent`)  
- **Event Store en MongoDB** con control optimista de concurrencia  
- **Publicación de eventos en Kafka** tras la persistencia  
- **Validaciones** con FluentValidation  
- **Módulo Common.Core** con contratos de eventos y mensajes compartibles  
- **Integración local con contenedores** (MongoDB replica set, Kafka)

---

## 🔄 Flujo resumido

1. Cliente → `POST /api/tickets`  
2. Endpoint crea `TicketCreateCommand` → MediatR  
3. Handler crea Aggregate y genera eventos con `RaiseEvent`  
4. EventStore verifica versión y persiste eventos en MongoDB  
5. Tras persistir, los eventos se publican en Kafka  

---

## ⚙️ Requisitos

- .NET 9 SDK  
- Docker / Docker Compose (para MongoDB replica set y Kafka)  
- Configuración en `appsettings.json`:  
  - Cadena de conexión Mongo (sin `directConnection=true`)  
  - Valores de Kafka (`host`, `port`, `topic`)  

---

## 🚀 Arranque rápido

1. Levantar contenedores:
   ```bash
   docker-compose up
2. Inicializar replica set en Mongo:
   rs.initiate()
3. Ajustar appsettings.json con host/ports correctos
4. Ejecutar la aplicación y probar:
   curl -X POST https://localhost:5001/api/tickets

✅ Buenas prácticas
Registrar IMongoClient como singleton en DI

No usar directConnection=true en replica sets

Habilitar retryReads / retryWrites y aplicar backoff ante errores transitorios

Mantener compatibilidad de eventos (Type/Version) al evolucionar el esquema

🤝 Contribución
Abrir issue describiendo bug o mejora

Crear rama feature/ o bugfix/ y enviar Pull Request con descripción

Ejecutar build y pruebas locales antes de PR

Licencia
Este proyecto se distribuye bajo la licencia MIT.
