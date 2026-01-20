package group

import (
	"avalonia_server/api/datastore"
	"github.com/gin-gonic/gin"
	"github.com/gogf/gf/v2/util/gconv"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
)

type JoinGroupChatParams struct {
	UserId   string `json:"user_id" form:"user_id" binding:"required"`
	GroupId  string `json:"group_id" form:"group_id"`
	FriendId string `json:"friend_id" form:"friend_id"`
}

func JoinGroupChatApi(r *gin.Context) {
	params := &JoinGroupChatParams{}
	if bindError := r.ShouldBind(params); bindError != nil {
		log.Println(bindError)
		r.JSON(http.StatusBadRequest, gin.H{"error": bindError.Error()})
		return
	}
	user := datastore.AllUsers[params.UserId]
	group := datastore.AllGroup[params.GroupId]
	flag1 := false
	flag2 := false

	if group != nil && user != nil {

		groupMember := &datastore.GroupMembers{
			Id:       user.Id,
			Name:     user.Name,
			Avatar:   user.Avatar,
			Usertype: "群员",
			Status:   "",
		}

		for _, val := range group.Members {
			if val.Id == params.UserId {
				flag1 = true
			}
		}
		if !flag1 {
			group.Members = append(group.Members, groupMember)

		}
		for _, val := range user.MessageGroups {
			if val == params.GroupId {
				flag2 = true
			}
		}
		if !flag2 {
			user.MessageGroups = append(user.MessageGroups, params.GroupId)

			send := datastore.WebSocketMessage{
				GroupId: params.GroupId,
				Type:    datastore.WsMsgJoinGroupChat,
				Data:    groupMember,
			}
			if err := user.Conn.WriteMessage(websocket.TextMessage, []byte(gconv.String(send))); err != nil {
				log.Println(err)
			}

		}
	}
	r.JSON(http.StatusOK, gin.H{})
}
