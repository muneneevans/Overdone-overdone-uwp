﻿using overdone_uwp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using overdone_uwp.Tile;
using System.Threading.Tasks;

namespace overdone_uwp.ViewModel
{
    public class AppViewModel : INotifyPropertyChanged 
    {
        private static AppViewModel _current;        
        public ObservableCollection<task> AllTasks { get; set; }
        public ObservableCollection<task> FolderTasks { get; set; }
        public ObservableCollection<folder> AllFolders { get; set; }
        public folder CurrentFolder { get; set; }
        DBManager DB = new DBManager();
        static MainPage _rootpage;

        private AppViewModel()
        {
            try
            {               
                AllTasks = new ObservableCollection<task>();            
                InitializeAllLists();
            }
            catch { }
        }
        public static AppViewModel GetViewModel()
        {
            return _current = _current == null ? new AppViewModel() : _current;
        }

        #region RootPage and Navigation
        public static void SetRootPage(MainPage mainpage)
        {
            _rootpage = mainpage;
        }

        public void NavigateTo<T>()
        {
            _rootpage.NavigateTo<T>();
        }

        public void NavigateTo<T>(object e)
        {
            _rootpage.NavigateTo<T>(e);
        }

        public void NavigateBack()
        {
            _rootpage.NavigateBack();
        }
        #endregion

        #region task List managers
        //function: create a new task
        public void AddTask(task NewTask)
        {
            try
            {
                NewTask.PropertyChanged += TaskPropertyChanged;
                AllTasks = DB.GetPendingTasksByDate(NewTask.task_deadline);
                AllTasks.Add(NewTask);
                NotifyPropertyChanged("AllTasks");
                DB.AddTask(NewTask);
                ToastManager.ToastManger.CreateCustomToast(NewTask, DateTime.Now.AddSeconds(1));
            }
            catch(Exception e) {
                e.Equals(e);
            }
        }
        //function: update a task
        public void UpdateTask(task SelectedTask)
        {
            try
            {
                try
                {
                    task t = AllTasks.Where(x => x.task_id == SelectedTask.task_id).FirstOrDefault();
                    t = SelectedTask;
                    NotifyPropertyChanged("AllTasks");
            }
            catch { }
                try
                {
                    task t = FolderTasks.Where(x => x.task_id == SelectedTask.task_id).FirstOrDefault();
                    t = SelectedTask;
                    NotifyPropertyChanged("FolderTasks");
        }
                catch { }
                DB.UpdateTask(SelectedTask);
            }
            catch { }
        }
        //function mark task as completed
        public void CompleteTask(task CompletedTask)
        {
            try
            {
                try
                {
                    //Remove from all tasks
                    AllTasks.Remove(CompletedTask);
                    NotifyPropertyChanged("AllTasks");
                }
                catch { }
                try
                {
                    //Remove from Folder tasks
                    FolderTasks.Remove(CompletedTask);
                    NotifyPropertyChanged("FolderTasks");
                }
                catch { }
                CompletedTask.task_status = true; 
                DB.UpdateTask(CompletedTask);
            }
            catch { }
        }
        //function: remove and deleta a task
        public void RemoveTask(task RemovedTask)
        {
            try
            {
                try
                {
                    AllTasks.Remove(RemovedTask);
                    NotifyPropertyChanged("AllTasks");
                }
                catch { }
                try
                {
                    FolderTasks.Remove(RemovedTask);
                    NotifyPropertyChanged("FolderTasks");
                }
                catch { }

                

                DB.DeleteTask(RemovedTask);

            }
            catch
            { }
        }
        #endregion        

        #region Folder Managers
        //function: add a new folder
        public void AddFolder(folder NewFolder)
        {
            try
            {
                AllFolders.Add(NewFolder);
                NotifyPropertyChanged("AllFolders");
                DB.AddFolder(NewFolder);
            }
            catch { }
        }
        //function: update a specific folder
        public void UpdateFolder(folder UpdatedFolder)
        {
            try
            {
                folder FoundFolder = (folder)AllFolders.Where(x => x.folder_id == UpdatedFolder.folder_id).FirstOrDefault();
                FoundFolder = UpdatedFolder;
                NotifyPropertyChanged("AllFolders");
                DB.UpdateFolder(UpdatedFolder);
            }
            catch
            { }
        }
        //function: remove and delete 
        public void RemoveFolder(folder RemovedFolder)
        {
            try
            {
                try
                {
                    AllFolders.Remove(RemovedFolder);
                    NotifyPropertyChanged("AllFolders");

                }
                catch
                { }

                DB.DeleteFolder(RemovedFolder);
            } catch { }
        }
        //function Set the current folder
        public void SetCurrentFolder(folder SelectedFolder)
        {
            try
            {
                CurrentFolder = SelectedFolder;
                FolderTasks = DB.GetTasksByFolder(SelectedFolder);
                NotifyPropertyChanged("FolderTasks");
                NotifyPropertyChanged("CurrentFolder");

            }
            catch { }
        }
        #endregion

        #region List Managers
        //function: initialize items of list and property listener
        public void InitializeAllLists()
        {
            try
            {
                AllFolders = DB.GetAllFolders();
                AllTasks = DB.GetPendingTasksByDate(DateTime.Now);
                ValidateRoutines();
                NotifyPropertyChanged("AllTasks");
                NotifyPropertyChanged("AllFolders");

                //PinTodaysTasksToTile();
                //PinAllPendingTasksToTile();           
            }
            catch
            {
            }
        }
        //function: initialize FolderTasksList
        public void InitializeFolderTasksList(folder SelectedFolder)
        {
            try
            {
                FolderTasks = new ObservableCollection<task>();
                FolderTasks = DB.GetTasksByFolder(SelectedFolder);
                SetPropertyListeners(FolderTasks);
                NotifyPropertyChanged("FolderTasks");
            }
            catch { }
        }
        //function: add property listeners to a task
        public void SetPropertyListeners(ObservableCollection<task> TaskList)
        {
            try
            {
                foreach (task t in TaskList)
                {
                    t.PropertyChanged += TaskPropertyChanged;
                }
            }
            catch { }
        }
        //function: order any list by date Descending
        public void SortListByDate(ObservableCollection<task> SelectedList)
        {
            try
            {
                SelectedList.OrderByDescending(x => x.task_deadline);
            }
            catch { }
        }
        //function: order list by status
        public void SortListByStatus(ObservableCollection<task> SelectedList)
        {
            try
            {
                SelectedList.OrderBy(x => x.task_status);
            }
            catch { }
        }
        //function: order list by folder
        public void SortListByFolder(ObservableCollection<task> SelectedList)
        {
            try
            {
                SelectedList.OrderBy(x => x.folder_id);
            }
            catch { }
            }
        //function: order list by status and folder
        public void SortListByStatusAndDate(ObservableCollection<task> SelectedList)
        {
            try
            {
                SelectedList.OrderBy(x => x.task_status).ThenByDescending(x => x.task_deadline);
            }
            catch { }
        }
        //function: Get Tasks based on date
        public void FilterTasksByDay(DateTime SelectedDate)
        {
            try
            {
                AllTasks = DB.GetPendingTasksByDate(SelectedDate);
                NotifyPropertyChanged("AllTasks");
            }
            catch { }
        }
        //function: Get and validate Routines
        public void ValidateRoutines()
        {
            try
            {
                ObservableCollection<task> AllRoutines = DB.GetRoutines();
                foreach (task t in AllRoutines)
                {
                    //check if the deadline has been met
                    if (t.task_deadline <= DateTime.Now)
                    {
                        if (t.task_status == true)
                        {
                            t.task_timesdone++;
                            t.task_status = false;
                            AllTasks.Add(t);
                        }
                        else
                        {
                            t.task_timesmissed++;
                        }
                        t.task_deadline = ComputeDeadline(t);
                        UpdateTask(t);
                        NotifyPropertyChanged("AllTasks");
                    }
                }
            }
            catch { }
        }
        //Cumpute next deadline for a routine
        public DateTime ComputeDeadline(task NewTask)
        {
            try
            {
                // get the type of the routine
                DateTime NewDeadline = DateTime.Now;
                var today = NewDeadline;
                var yesterday = NewDeadline.AddDays(-1);
                var thisWeekStart = NewDeadline.AddDays(-(int)NewDeadline.DayOfWeek);
                var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
                var lastWeekStart = thisWeekStart.AddDays(-7);
                var lastWeekEnd = thisWeekStart.AddSeconds(-1);
                var thisMonthStart = NewDeadline.AddDays(1 - NewDeadline.Day);
                var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);
                var lastMonthEnd = thisMonthStart.AddSeconds(-1);
                var nextMonthStart = thisMonthStart.AddMonths(1);
                var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

                DateTime ReturnDate = NewDeadline;

                switch (NewTask.task_interval)
                {
                    case 1:
                        //daily move deadline to next day
                        ReturnDate = new DateTime(NewDeadline.Year, NewDeadline.Month, NewDeadline.Day, NewTask.task_deadline.Hour, NewTask.task_deadline.Minute, NewTask.task_deadline.Second);
                        break;
                    case 2:
                        ReturnDate = new DateTime(thisWeekEnd.Year, thisWeekEnd.Month, ((NewTask.task_deadline).AddDays(7)).Day, NewTask.task_deadline.Hour, NewTask.task_deadline.Minute, NewTask.task_deadline.Second);
                        break;
                    case 3:
                        ReturnDate = new DateTime(nextMonthEnd.Year, nextMonthEnd.Month, nextMonthEnd.Day, NewTask.task_deadline.Hour, NewTask.task_deadline.Minute, NewTask.task_deadline.Second);
                        break;
                    default:
                        break;
                }
                return ReturnDate;
            }
            catch { return new DateTime(); }
        }
        #endregion

        #region Notify Event Managers
        //variable: event raised when a class property changes
        public event PropertyChangedEventHandler PropertyChanged;
        //function: default property for to notify property
        public void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //function event handler when a task is modified
        private void TaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "task_status")
                {                    
                }
            }
            catch { }
        }
        #endregion

        #region Notification Manager
        //function Show Folder in tile
        public async void PinFolderToTile(folder SelectedFolder)
        {
            try
            {
                await TileManager.DefaultTile(DB.GetPendingTasksByFolder(SelectedFolder).ToList(), SelectedFolder.folder_name);
            }
            catch { }
        }
        //function Show Current days task on Live Tile
        public async void PinDateTasksToTile(DateTime SelectedDate)
        {
            try
            {
                await TileManager.DefaultTile(DB.GetPendingTasksByDate(SelectedDate).ToList(), SelectedDate.Date.Day.ToString());
            }
            catch { }
        }
        //function SHow Todays Tasks
        public async void PinTodaysTasksToTile()
        {
            try
            {
                await TileManager.DefaultTile(DB.GetPendingTasksByDate(DateTime.Now.Date).ToList(), "Today");
            }
            catch { }
        }
        //function: pin all tasks to tile
        public async void PinAllPendingTasksToTile()
        {
            try
            {
                await TileManager.DefaultTile(DB.GetPendingTasks().ToList());
            }
            catch (Exception)
            {

                //throw;
            }
        }
        #endregion

    }
}
