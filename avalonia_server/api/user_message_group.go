package api

import (
	"github.com/gin-gonic/gin"
)

func UserMessageGroupApi(r *gin.Context) {

	data := []*UserMessageGroup{}
	for _, group := range AllGroup {
		data = append(data, group)
	}

	r.JSON(200, gin.H{
		"code": 200,
		"msg":  "success",
		"data": data,
	})
}
