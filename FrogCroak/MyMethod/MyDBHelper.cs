using System;
using Android.Content;
using Android.Database.Sqlite;

namespace FrogCroak.MyMethod
{
    public class MyDBHelper : SQLiteOpenHelper
    {
        public MyDBHelper(Context context, String name, SQLiteDatabase.ICursorFactory factory, int version)
            :base(context, name, factory, version)
        {
            
        }

        public override void OnCreate(SQLiteDatabase db)
        {
            db.ExecSQL("CREATE TABLE chat " +
                "(_id INTEGER PRIMARY KEY  NOT NULL , " +
                "message TEXT NOT NULL , " +
                "isme INTEGER NOT NULL , " +
                "type INTEGER NOT NULL)");

            db.ExecSQL("CREATE TABLE marker " +
                "(_id INTEGER PRIMARY KEY  NOT NULL , " +
                "latitude REAL NOT NULL , " +
                "longitude REAL NOT NULL , " +
                "title TEXT NOT NULL , " +
                "content TEXT NOT NULL)");
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            throw new NotImplementedException();
        }
    }
}