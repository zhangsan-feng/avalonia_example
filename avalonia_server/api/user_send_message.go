package api

import (
	"github.com/gin-gonic/gin"
	"github.com/gogf/gf/v2/os/gfile"
	"github.com/gogf/gf/v2/util/gconv"
	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
	"time"
)

type UserSendMessage struct {
	SendUserId  string `form:"send_user_id" binding:"required"`
	SendGroupId string `form:"send_group_id" binding:"required"`
	Message     string `form:"message"`
}

func UserSendMessageApi(r *gin.Context) {
	var req *UserSendMessage
	err := r.ShouldBind(&req)
	if err != nil {
		log.Println(err)
		return
	}

	log.Println(req.SendUserId, req.SendGroupId, req.Message)

	form, _ := r.MultipartForm()
	files := form.File["files"]
	//log.Println(files)

	message_files := []string{}

	if len(files) > 0 {
		for _, fileHeader := range files {
			tmp_file_path := "./static/files/" + fileHeader.Filename
			message_files = append(message_files, "http://127.0.0.1:34332/avatar/files/"+fileHeader.Filename)
			if gfile.Exists(tmp_file_path) {
				continue
			}
			if fileHeader.Size > 8*1024*1024 {
				continue
			}
			err := r.SaveUploadedFile(fileHeader, tmp_file_path)
			if err != nil {
				r.JSON(http.StatusInternalServerError, gin.H{"error": "failed to save file"})
				return
			}
		}
	}

	//AllUsers[req.SendUserId].Conn.WriteMessage(websocket.TextMessage, []byte(req.Message))
	//if err := global.EventBus.Publish(event_bus.EventWebSocketMessage, gconv.String("")); err != nil {
	//	log.Println(err)
	//}

	//log.Println(message_files)

	group := AllGroup[req.SendGroupId]
	exist := false
	for _, groupMember := range group.Members {
		if groupMember.Id == req.SendUserId {
			exist = true
		}
	}
	if !exist {
		groupMember := &GroupMembers{
			Id:       req.SendUserId,
			Name:     AllUsers[req.SendUserId].Name,
			Avatar:   AllUsers[req.SendUserId].Avatar,
			Usertype: "普通群员",
			Status:   AllUsers[req.SendUserId].Status,
		}

		group.Members = append(group.Members, groupMember)
		for _, v := range group.Members {
			if AllUsers[v.Id] != nil {
				if AllUsers[v.Id].Conn != nil {
					send := &WebSocketMessage{
						Type:    "join_group",
						Data:    groupMember,
						GroupId: req.SendGroupId,
					}
					if err := AllUsers[v.Id].Conn.WriteMessage(websocket.TextMessage, []byte(gconv.String(send))); err != nil {
						log.Println(err)
					}
				}
			}
		}

	}

	for _, v := range group.Members {
		if AllUsers[v.Id] != nil {
			if AllUsers[v.Id].Conn != nil {
				sendMsg := &GroupHistory{
					MessageId:      uuid.New().String(),
					SendGroupId:    req.SendGroupId,
					SendUserId:     req.SendUserId,
					SendUserName:   AllUsers[req.SendUserId].Name,
					SendUserAvatar: AllUsers[req.SendUserId].Avatar,
					Message:        req.Message,
					Time:           time.Now().Format("2006-01-02 15:04:05"),
					Files:          message_files,
				}
				//log.Println(gconv.String(sendMsg))
				send := &WebSocketMessage{
					Type: "message",
					Data: sendMsg,
				}
				if err := AllUsers[v.Id].Conn.WriteMessage(websocket.TextMessage, []byte(gconv.String(send))); err != nil {
					log.Println(err)
				}
				if len(group.History) == 1000 {
					group.History = group.History[500:]
				}
				group.History = append(group.History, sendMsg)
			}
		}
	}
	//log.Println(data)
}
