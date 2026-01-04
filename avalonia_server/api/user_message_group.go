package api

import (
	"github.com/gin-gonic/gin"
	"log"
)

func UserMessageGroupApi(r *gin.Context) {
	user_id := r.Query("user_id")
	log.Println(user_id)
	m_g := ActiveGroup[user_id]

	log.Println(m_g)
	r.JSON(200, gin.H{
		"code": 200,
		"msg":  "success",
		"data": m_g,
	})
}
