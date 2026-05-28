using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ItemDisplayPlacementHelper.Dialogs
{
    public delegate void FilePathPickHandler(string path);
    public class FilePathPickerDialogController : MonoBehaviour
    {
        public TMP_InputField pathField;
        public TMP_InputField fileField;
        public TMP_Text fileFieldHeader;
        public TMP_Text dialogHeader;
        public GameObject rowPrefab;
        public Transform container;

        public Sprite folderIcon;
        public Sprite fileIcon;

        public FilePathPickHandler onPick;
        [NonSerialized]
        public bool selectFile;
        [NonSerialized]
        public bool allowNotExisting;
        [NonSerialized]
        public string fileExtension;

        private string lastValidPath;
        private readonly List<FileRowController> rows = new List<FileRowController>();

        public void Start()
        {
            lastValidPath = ConfigHelper.FilePickerLastPath.Value ?? "";
            if (!Directory.Exists(lastValidPath))
            {
                lastValidPath = Environment.CurrentDirectory;
            }
            pathField.text = lastValidPath;
            Refresh();

            if (selectFile)
            {
                fileFieldHeader.text = "File";
                dialogHeader.text = "Select file";
            }
            else
            {
                fileFieldHeader.text = "Folder";
                dialogHeader.text = "Select folder";
            }

            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidFileChars = Path.GetInvalidFileNameChars();
            pathField.onValidateInput += (text, pos, ch) =>
            {
                if (invalidPathChars.Contains(ch))
                {
                    return default;
                }

                return ch;
            };
            fileField.onValidateInput += (text, pos, ch) =>
            {
                if (invalidFileChars.Contains(ch))
                {
                    return default;
                }

                return ch;
            };
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(lastValidPath))
            {
                var drives = DriveInfo.GetDrives();
                EnsureRowCount(drives.Length);
                for (var i = 0; i < drives.Length; i++)
                {
                    var row = rows[i];
                    row.text.text = drives[i].VolumeLabel;
                    row.part = drives[i].RootDirectory.Name;
                    row.icon.sprite = folderIcon;
                    row.isFile = false;
                }
                return;
            }

            var directories = Directory.GetDirectories(lastValidPath, "*", new EnumerationOptions { IgnoreInaccessible = true });
            var files = selectFile ? Directory.GetFiles(lastValidPath, string.IsNullOrEmpty(fileExtension) ? "*" : $"*.{fileExtension}", new EnumerationOptions { IgnoreInaccessible = true }) : Array.Empty<string>();
            EnsureRowCount(directories.Length + files.Length);

            for (var i = 0; i < directories.Length; i++)
            {
                var row = rows[i];
                row.text.text = Path.GetFileName(directories[i]);
                row.part = row.text.text;
                row.icon.sprite = folderIcon;
                row.isFile = false;
            }

            for (var i = 0; i < files.Length; i++)
            {
                var row = rows[directories.Length + i];
                row.text.text = Path.GetFileName(files[i]);
                row.part = row.text.text;
                row.icon.sprite = fileIcon;
                row.isFile = true;
            }
        }

        private void EnsureRowCount(int length)
        {
            if (rows.Count < length)
            {
                for (var i = rows.Count; i < length; i++)
                {
                    var row = Instantiate(rowPrefab, container);
                    row.SetActive(true);
                    rows.Add(row.GetComponent<FileRowController>());
                }
            }
            else
            {
                for (var i = rows.Count - 1; i >= length; i--)
                {
                    Destroy(rows[i].gameObject);
                    rows.RemoveAt(i);
                }
            }
        }

        public void ValidatePath()
        {
            if (Directory.Exists(pathField.text))
            {
                if (pathField.text.EndsWith(':'))
                {
                    lastValidPath = $"{pathField.text}{Path.DirectorySeparatorChar}";
                }
                else
                {
                    lastValidPath = pathField.text;
                }
                Refresh();
            }
            else
            {
                pathField.text = lastValidPath;
            }
        }

        public void SelectRow(FileRowController row)
        {
            if (!row.isFile)
            {
                EventSystem.current.SetSelectedGameObject(null);
                lastValidPath = string.IsNullOrEmpty(lastValidPath) ? row.part : Path.Combine(lastValidPath, row.part);
                pathField.text = lastValidPath;
                if (!selectFile)
                {
                    fileField.text = "";
                }
                Refresh();
            }
            else if (row.isFile == selectFile)
            {
                fileField.text = row.part;
            }
        }

        public void Up()
        {
            lastValidPath = Path.GetDirectoryName(lastValidPath);
            pathField.text = lastValidPath;
            Refresh();
        }

        public void Ok()
        {
            var fullPath = string.IsNullOrEmpty(fileField.text) ? pathField.text : Path.Combine(pathField.text, fileField.text);

            if (selectFile)
            {
                if (string.IsNullOrEmpty(fileField.text))
                {
                    DialogController.ShowError("File name is required");
                    return;
                }
                if (!allowNotExisting && !File.Exists(fullPath))
                {
                    DialogController.ShowError("File does not exist");
                    return;
                }
                ConfigHelper.FilePickerLastPath.Value = pathField.text;
            }
            else
            {
                if (!allowNotExisting && !Directory.Exists(fullPath))
                {
                    DialogController.ShowError("Directory does not exist");
                    return;
                }
                ConfigHelper.FilePickerLastPath.Value = fullPath;
            }

            onPick?.Invoke(fullPath);

            DialogController.HideTopmost();
        }

        public void Cancel()
        {
            DialogController.HideTopmost();
        }
    }
}
