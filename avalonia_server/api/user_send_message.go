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
	log.Println(files)

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

	//if err := global.EventBus.Publish(event_bus.EventWebSocketMessage, gconv.String("")); err != nil {
	//	log.Println(err)
	//}
	log.Println(message_files)

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
							Files:          message_files,
						}
						log.Println(gconv.String(sendMsg))
						err := ActiveUsers[k.Id].Conn.WriteMessage(websocket.TextMessage, []byte(gconv.String(sendMsg)))
						if err != nil {
							log.Println(err)
							return
						}
						if len(v.History) == 1000 {
							v.History = v.History[500:]
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
