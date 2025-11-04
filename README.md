# Notifications start

cd Scripts
./build.sh --dev
./up.sh --dev

старт фронта:
npm run dev

# Tools
## Swagger
Если захочется потестить в swagger-ui, в поле вставки токена надо написать именно 'Bearer {token}', а не просто токен

## CLI PostgreSQL
Подключение к CLI:
`docker exec -it <имя_контейнера> psql -U postgres`

В Docker Desktop:
`psql -U postgres`

Получение списка баз данных:
`\l` или `\list`
`\l+` для более детальной информации

Подключение к базе данных:
`\c {имя базы данных}`

Просмотр таблиц
`\dt`
`\dt+` для более детальной информации

Получение данных:
```sql
SELECT * FROM users;
```
Одной командой:
`docker exec -it <имя_контейнера> psql -U <пользователь> -d <имя_базы> -c "<SQL-запрос>"`

В Docker Desktop:
`psql -U <пользователь> -d <имя_базы> -c "<SQL-запрос>"`

## Testing SignalR connection in DevTools
```
const token = 'YOUR_FULL_TOKEN';

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`http://localhost:5002/notificationHub?access_token=${encodeURIComponent(token)}`)
  .configureLogging(signalR.LogLevel.Information) // Детальные логи: Negotiate, Errors
  .build();

connection.start()
  .then(() => {
    console.log('✅ Connected to SignalR Hub');
    // Invoke RequestUnreadCount
    return connection.invoke('RequestUnreadCount');
  })
  .then(() => {
    console.log('✅ RequestUnreadCount invoked');
  })
  .catch((err) => {
    console.error('❌ Connection Error:', err);
  });

// Listen для сообщений
connection.on('ReceiveUnreadCount', (count) => {
  console.log('📩 Unread Count received:', count); // От хаба или sender
});

connection.onclose((err) => {
  console.log('🔌 Connection closed:', err ? err.message : 'OK');
});

// Авто-disconnect через 1 мин
setTimeout(() => connection.stop(), 60000);

```