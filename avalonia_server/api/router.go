package api

import (
	"github.com/gin-gonic/gin"
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
			for k := range ActiveUsers {
				//log.Println(ActiveUsers[k])
				if ActiveUsers[k].Conn == nil {
					delete(ActiveUsers, k)
					continue
				}
				if err := ActiveUsers[k].Conn.WriteMessage(websocket.PingMessage, []byte("ping")); err != nil {
					delete(ActiveUsers, k)
					log.Printf("发送 ping 失败: %v", err)
				}

			}
		}
	}
}

func NewHttpRouter(r *gin.Engine) {
	r.Static("/avatar", "./static")
	r.GET("/user_message_group", UserMessageGroupApi)

	r.POST("/user_send_message", UserSendMessageApi)
	r.GET("/register_ws", RegisterWsConn)
}
