package api

import (
	"github.com/gin-gonic/gin"
	"github.com/gogf/gf/v2/util/gconv"
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"log"
	"time"
)

type UserSendMessage struct {
	SendUserId  string   `json:"send_user_id"`
	SendGroupId string   `json:"send_group_id"`
	Message     string   `json:"message"`
	Emoji       []string `json:"emoji"`
}

func UserSendMessageApi(r *gin.Context) {
	var req *UserSendMessage
	err := r.BindJSON(&req)
	if err != nil {
		log.Println(err)
		return
	}

	log.Println(req.SendUserId, req.SendGroupId, req.Message)

	//if err := global.EventBus.Publish(event_bus.EventWebSocketMessage, gconv.String("")); err != nil {
	//	log.Println(err)
	//}
	//ActiveUsers[req.SendUserId].Conn.WriteMessage(websocket.TextMessage, []byte(req.Message))

	for _, v := range ActiveGroup[req.SendUserId] {
		if v.ID == req.SendGroupId {
			for _, k := range v.Members {
				if ActiveUsers[k.Id] != nil {
					if ActiveUsers[k.Id].Conn != nil {
						uid, uuid_err := uuid.NewUUID()
						if uuid_err != nil {
							log.Println(uuid_err)
							return
						}
						sendMsg := &GroupHistoryMessage{
							MessageId:      uid.String(),
							SendGroupId:    req.SendGroupId,
							SendUserId:     req.SendUserId,
							SendUserName:   AllUsers[req.SendUserId].Name,
							SendUserAvatar: AllUsers[req.SendUserId].Avatar,
							Message:        req.Message,
							Time:           time.Now().Format("2006-01-02 15:04:05"),
							Emoji:          nil,
						}

						err := ActiveUsers[k.Id].Conn.WriteMessage(websocket.TextMessage, []byte(gconv.String(sendMsg)))
						if err != nil {
							log.Println(err)
							return
						}
						v.History = append(v.History, sendMsg)
					}
				}
			}
			break
		}
	}
	//log.Println(data)
}
