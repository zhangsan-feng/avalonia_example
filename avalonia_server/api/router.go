package api

import (
	"github.com/gin-gonic/gin"
)

func NewHttpRouter(r *gin.Engine) {
	r.Static("/avatar", "./static")
	r.GET("/user_message_group", UserMessageGroupApi)

	r.POST("/user_send_message", UserSendMessageApi)
	r.GET("/register_ws", RegisterWsConn)
	r.POST("/user/login", LoginApi)
}
