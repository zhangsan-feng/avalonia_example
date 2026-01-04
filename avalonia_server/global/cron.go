package global

import (
	"avalonia_server/event_bus"
	"github.com/robfig/cron/v3"
)

type CrontabTask struct {
	CronId cron.EntryID
	Task   *cron.Cron
	Fn     func()
}

func NewCron() *CrontabTask {
	return &CrontabTask{}
}

func (c *CrontabTask) AddTask() {
}

func DelTask() {

}

var EventBus = event_bus.New()
