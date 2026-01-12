package api

import (
	"github.com/gorilla/websocket"
	"log"
	"time"
)

func CheckWebsocketConn() {
	ticker := time.NewTicker(5 * time.Second)
	defer ticker.Stop()

	defer func() {
		if err := recover(); err != nil {
			log.Println(err)
		}
	}()

	for {
		select {
		case <-ticker.C:
			for k := range AllUsers {
				//log.Println(ActiveUsers[k])
				if AllUsers[k].Conn == nil {
					AllUsers[k].Status = "离线"
					continue
				}
				if err := AllUsers[k].Conn.WriteMessage(websocket.PingMessage, []byte("ping")); err != nil {
					AllUsers[k].Status = "离线"
					//log.Printf("发送 ping 失败: %v", err)
				}

			}
		}
	}
}
